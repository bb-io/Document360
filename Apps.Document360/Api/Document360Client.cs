using System.Net;
using Apps.Document360.Constants;
using Apps.Document360.Models.Dtos;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RestSharp;

namespace Apps.Document360.Api;

/// <summary>
/// Transport for the Document360 Customer API v2. It owns authentication, the response envelope, paging and the
/// translation of failures into Blackbird exceptions — and nothing about articles, workspaces or any other kind of
/// content. Those live in the services under <c>Api/Services</c>.
/// </summary>
/// <remarks>
/// Three vendor quirks are handled here: every payload is wrapped in an envelope, a failed operation can arrive
/// with HTTP 200 and <c>success: false</c>, and an invalid token answers 401 with a plain-text body.
/// </remarks>
public class Document360Client : BlackBirdRestClient
{
    /// <summary>The largest page size the paged endpoints accept.</summary>
    public const int MaxPageSize = 100;

    private const int RetryCount = 8;
    private const int BaseBackoffSeconds = 1;
    private const int MaxBackoffSeconds = 16;

    /// <summary>
    /// Document360 allows roughly 100 requests per window, which a workflow over a few dozen articles walks straight
    /// through. A throttled request is backed off and retried rather than failing the run.
    /// </summary>
    private readonly AsyncRetryPolicy<RestResponse> retryPolicy = Policy
        .HandleResult<RestResponse>(response => response.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(RetryCount, (retryAttempt, result, _) => GetRetryDelay(retryAttempt, result.Result),
            (_, _, _, _) => Task.CompletedTask);

    public Document360Client(IEnumerable<AuthenticationCredentialsProvider> creds) : base(new RestClientOptions
    {
        BaseUrl = ResolveBaseUrl(creds),
        Timeout = TimeSpan.FromSeconds(180)
    })
    {
        ApiHubUrl = ResolveBaseUrl(creds);

        // The token contains '+', '/' and '=' and must travel verbatim.
        this.AddDefaultHeader("api_token", ResolveApiToken(creds));
        this.AddDefaultHeader("Accept", "application/json");
    }

    /// <summary>The regional API hub this connection talks to.</summary>
    public Uri ApiHubUrl { get; }

    /// <summary>Sends the request and returns the payload, failing when Document360 reports no data.</summary>
    public async Task<T> GetDataAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
        where T : class
    {
        var envelope = await SendAsync<T>(request, cancellationToken);

        return envelope.Payload ??
               throw new PluginApplicationException(
                   "Document360 reported success but returned no data. Please try running the action again.");
    }

    /// <summary>Sends the request and returns the payload as a list, empty when there is nothing to return.</summary>
    public async Task<List<T>> GetListAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
    {
        var envelope = await SendAsync<List<T>>(request, cancellationToken);
        return envelope.Payload ?? [];
    }

    /// <summary>
    /// Walks a page-based endpoint until it reports no more results. The caller builds each page's request so it
    /// can add its own query parameters; the zero-based page index is handed in.
    /// </summary>
    public async Task<List<T>> GetPagedListAsync<T>(Func<int, RestRequest> buildRequest, int maxPages,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();

        for (var page = 0; page < maxPages; page++)
        {
            var envelope = await SendAsync<List<T>>(buildRequest(page), cancellationToken);
            var items = envelope.Payload ?? [];
            results.AddRange(items);

            if (items.Count == 0 || envelope.Pagination is null || !envelope.Pagination.HasMore) break;
        }

        return results;
    }

    /// <summary>
    /// Walks a skip/take endpoint. The page size Document360 actually honours is learnt from the first response,
    /// because it caps <c>take</c> server-side without saying so.
    /// </summary>
    public async Task<List<T>> GetSkipTakeListAsync<T>(Func<int, int, RestRequest> buildRequest, int pageSize,
        int maxPages, CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        int? honouredPageSize = null;

        for (var page = 0; page < maxPages; page++)
        {
            var items = await GetListAsync<T>(buildRequest(results.Count, pageSize), cancellationToken);
            if (items.Count == 0) break;

            results.AddRange(items);
            honouredPageSize ??= items.Count;

            if (items.Count < honouredPageSize) break;
        }

        return results;
    }

    /// <summary>Sends the request and fails when the operation did not succeed.</summary>
    public async Task<Document360Response<T>> SendAsync<T>(RestRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithRetryAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) throw ConfigureErrorException(response);

        var envelope = ParseEnvelope<T>(response);
        if (!envelope.Success) throw BuildFailureException(envelope.ErrorMessage);

        return envelope;
    }

    /// <summary>
    /// Sends the request and hands back the envelope even when the operation failed, so a caller can react to a
    /// particular vendor message instead of surfacing it. Anything that is not a readable business failure — a
    /// rejected token, an outage, an unreachable host — is still raised.
    /// </summary>
    public async Task<Document360Response<T>> SendUncheckedAsync<T>(RestRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithRetryAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode) return ParseEnvelope<T>(response);

        if (IsBusinessFailure(response))
        {
            var envelope = TryParseEnvelope<T>(response);
            if (envelope is not null && envelope.HasErrorDetails) return envelope;
        }

        throw ConfigureErrorException(response);
    }

    /// <summary>Turns a vendor failure message into the exception the user should see.</summary>
    public Exception BuildFailureException(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return new PluginApplicationException(
                "Document360 rejected the request without giving a reason. Please try running the action again.");

        var hint = message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            ? " Check the workspace, article, locale and team account you selected."
            : string.Empty;

        return new PluginMisconfigurationException($"Document360 could not complete the request: {message}{hint}");
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        // An invalid or missing token answers 401 with the plain text "Invalid API token" — no JSON to parse.
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return new PluginMisconfigurationException(
                "Document360 rejected the API token. Check the token in your connection and make sure the API hub " +
                "URL matches the region your knowledge base is hosted in.");

        if (response.StatusCode is HttpStatusCode.TooManyRequests)
            return new PluginApplicationException(
                "Document360 rate limit reached. Wait a moment before running the workflow again.");

        var message = ExtractErrorMessage(response);

        if ((int)response.StatusCode >= 500)
            return new PluginApplicationException(
                $"Document360 is currently unavailable ({(int)response.StatusCode}). {message}");

        return BuildFailureException(message);
    }

    /// <summary>The single point every request passes through, so throttling is handled once for the whole app.</summary>
    private Task<RestResponse> ExecuteWithRetryAsync(RestRequest request, CancellationToken cancellationToken) =>
        retryPolicy.ExecuteAsync(() => ExecuteAsync(request, cancellationToken));

    private static TimeSpan GetRetryDelay(int retryAttempt, RestResponse? response)
    {
        // The server's own advice comes first when it gives any.
        var retryAfter = response?.Headers?
            .FirstOrDefault(header => string.Equals(header.Name, "Retry-After", StringComparison.OrdinalIgnoreCase))?
            .Value?.ToString();

        if (int.TryParse(retryAfter, out var seconds) && seconds > 0) return TimeSpan.FromSeconds(seconds);

        // Otherwise back off exponentially, capped, to ride out the window rather than walking back into it.
        var backoff = Math.Min(BaseBackoffSeconds * Math.Pow(2, retryAttempt - 1), MaxBackoffSeconds);

        // Jitter keeps several workflows throttled at the same moment from retrying in lockstep.
        return TimeSpan.FromSeconds(backoff) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
    }

    private static bool IsBusinessFailure(RestResponse response) =>
        (int)response.StatusCode is >= 400 and < 500 &&
        response.StatusCode is not (HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
            or HttpStatusCode.TooManyRequests);

    private static Document360Response<T> ParseEnvelope<T>(RestResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Content))
            throw new PluginApplicationException(
                "Document360 returned an empty response. Please try running the action again.");

        return TryParseEnvelope<T>(response) ?? throw new PluginApplicationException(
            $"Document360 returned an unexpected response: {Truncate(response.Content)}");
    }

    private static Document360Response<T>? TryParseEnvelope<T>(RestResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Content)) return null;

        try
        {
            return JsonConvert.DeserializeObject<Document360Response<T>>(response.Content);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ExtractErrorMessage(RestResponse response)
    {
        var envelopeMessage = TryParseEnvelope<object>(response)?.ErrorMessage;
        if (!string.IsNullOrWhiteSpace(envelopeMessage)) return envelopeMessage;

        if (!string.IsNullOrWhiteSpace(response.Content)) return Truncate(response.Content);

        return response.ErrorMessage ?? response.StatusDescription ?? response.StatusCode.ToString();
    }

    private static string Truncate(string value) => value.Length <= 300 ? value : value[..300] + "...";

    private static Uri ResolveBaseUrl(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var value = creds.FirstOrDefault(x => x.KeyName == CredsNames.BaseUrl)?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException(
                "The API hub URL is missing from the connection. Reconnect and choose the API hub of your region, " +
                "for example https://apihub.us.document360.io.");

        if (!Uri.TryCreate(value.TrimEnd('/'), UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https"))
            throw new PluginMisconfigurationException(
                $"'{value}' is not a valid API hub URL. Use the full address of your region's hub, " +
                "for example https://apihub.us.document360.io.");

        return uri;
    }

    private static string ResolveApiToken(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var token = creds.FirstOrDefault(x => x.KeyName == CredsNames.ApiToken)?.Value;

        if (string.IsNullOrWhiteSpace(token))
            throw new PluginMisconfigurationException(
                "The API token is missing from the connection. Reconnect and paste a token created in the " +
                "Document360 portal under Settings > Knowledge base portal > API tokens.");

        return token;
    }
}
