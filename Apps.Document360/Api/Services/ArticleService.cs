using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Api.Services;

/// <summary>Every article endpoint of the Customer API, including the ones with non-obvious failure behaviour.</summary>
public class ArticleService(Document360Client client)
{
    private const int MaxArticlePages = 50;

    public Task<List<ArticleListItemDto>> GetArticlesAsync(string workspaceId, string? langCode = null,
        int? securityVisibility = null, int maxPages = MaxArticlePages, CancellationToken cancellationToken = default)
    {
        return client.GetPagedListAsync<ArticleListItemDto>(page =>
        {
            var request = new RestRequest($"/v2/ProjectVersions/{workspaceId}/articles", Method.Get)
                .AddQueryParameter("page", page.ToString())
                .AddQueryParameter("hitsPerPage", Document360Client.MaxPageSize.ToString());

            if (!string.IsNullOrWhiteSpace(langCode)) request.AddQueryParameter("langCode", langCode);
            if (securityVisibility.HasValue)
                request.AddQueryParameter("securityVisibility", securityVisibility.Value.ToString());

            return request;
        }, maxPages, cancellationToken);
    }

    public Task<ArticleDto> GetArticleAsync(string articleId, string langCode, bool isPublished = false,
        bool isForDisplay = false, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}", Method.Get)
            .AddQueryParameter("isForDisplay", isForDisplay.ToString().ToLowerInvariant())
            .AddQueryParameter("isPublished", isPublished.ToString().ToLowerInvariant())
            .AddQueryParameter("appendSASToken", "true");

        return client.GetDataAsync<ArticleDto>(request, cancellationToken);
    }

    public Task<ArticleDto> GetArticleVersionAsync(string articleId, string langCode, int versionNumber,
        bool isForDisplay = false, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}/versions/{versionNumber}", Method.Get)
            .AddQueryParameter("isForDisplay", isForDisplay.ToString().ToLowerInvariant())
            .AddQueryParameter("appendSASToken", "true");

        return client.GetDataAsync<ArticleDto>(request, cancellationToken);
    }

    public Task<List<ArticleVersionDto>> GetArticleVersionsAsync(string articleId, string langCode,
        CancellationToken cancellationToken = default) =>
        client.GetListAsync<ArticleVersionDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/versions", Method.Get), cancellationToken);

    public Task<ArticleSettingsDto> GetArticleSettingsAsync(string articleId, string langCode,
        CancellationToken cancellationToken = default) =>
        client.GetDataAsync<ArticleSettingsDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/settings", Method.Get), cancellationToken);

    public Task<CreatedArticleDto> CreateArticleAsync(Dictionary<string, object> body,
        CancellationToken cancellationToken = default) =>
        client.GetDataAsync<CreatedArticleDto>(new RestRequest("/v2/Articles", Method.Post).AddJsonBody(body),
            cancellationToken);

    /// <summary>
    /// Writes title, content, placement and translation status. Returns <c>false</c> — instead of throwing — when
    /// Document360 refuses the write because the target version is read-only, which is the caller's signal to copy
    /// the version into a new draft and try again.
    /// </summary>
    public async Task<bool> TryUpdateArticleAsync(string articleId, string langCode, Dictionary<string, object> body,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}", Method.Put).AddJsonBody(body);
        var envelope = await client.SendUncheckedAsync<ArticleDto>(request, cancellationToken);

        if (envelope.Success) return true;
        if (RequiresNewDraft(envelope.ErrorMessage)) return false;

        throw client.BuildFailureException(envelope.ErrorMessage);
    }

    /// <summary>Creates a new writable draft version of an article translation.</summary>
    public Task<ArticleVersionDto> ForkArticleAsync(string articleId, Dictionary<string, object> body,
        CancellationToken cancellationToken = default) =>
        client.GetDataAsync<ArticleVersionDto>(
            new RestRequest($"/v2/Articles/{articleId}/fork", Method.Put).AddJsonBody(body), cancellationToken);

    /// <summary>
    /// Sets the article description. The value travels as a query parameter, and re-sending an unchanged
    /// description answers <c>success: false</c> with an empty error list, which is a no-op rather than a failure.
    /// </summary>
    public async Task UpdateArticleDescriptionAsync(string articleId, string langCode, string description,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}/updateDescription", Method.Put)
            .AddQueryParameter("description", description);

        var envelope = await client.SendUncheckedAsync<object>(request, cancellationToken);

        if (!envelope.Success && envelope.HasErrorDetails)
            throw client.BuildFailureException(envelope.ErrorMessage);
    }

    public Task<ArticleSettingsDto> UpdateArticleSettingsAsync(string articleId, string langCode,
        Dictionary<string, object> body, CancellationToken cancellationToken = default) =>
        client.GetDataAsync<ArticleSettingsDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/settings", Method.Put).AddJsonBody(body),
            cancellationToken);

    /// <summary>
    /// Publishes a version. The public URL sits at the envelope root rather than inside <c>data</c>. Publishing a
    /// version that is already live is reported as an outcome instead of raised, so re-running a workflow is safe.
    /// </summary>
    public async Task<PublishArticleResult> PublishArticleAsync(string articleId, string langCode,
        Dictionary<string, object> body, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}/publish", Method.Post).AddJsonBody(body);
        var envelope = await client.SendUncheckedAsync<object>(request, cancellationToken);

        if (envelope.Success) return new PublishArticleResult(false, envelope.Url);
        if (IsAlreadyPublished(envelope.ErrorMessage)) return new PublishArticleResult(true, null);

        throw client.BuildFailureException(envelope.ErrorMessage);
    }

    /// <summary>The IDs of the articles that are in a given translation state for a locale.</summary>
    public async Task<HashSet<string>> GetTranslationArticleIdsAsync(string workspaceId, string langCode, int status,
        CancellationToken cancellationToken = default)
    {
        var request = new RestRequest($"/v2/Translations/{workspaceId}/{langCode}", Method.Get)
            .AddQueryParameter("status", status.ToString());

        var entries = await client.GetListAsync<TranslationEntryDto>(request, cancellationToken);
        return entries.Select(entry => entry.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool RequiresNewDraft(string? message) =>
        message is not null &&
        (message.Contains("fork the article", StringComparison.OrdinalIgnoreCase) ||
         message.Contains("read-only", StringComparison.OrdinalIgnoreCase) ||
         message.Contains("read only", StringComparison.OrdinalIgnoreCase));

    private static bool IsAlreadyPublished(string? message) =>
        message is not null && message.Contains("already been published", StringComparison.OrdinalIgnoreCase);
}
