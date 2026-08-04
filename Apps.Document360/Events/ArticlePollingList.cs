using Apps.Document360.Api;
using Apps.Document360.Models.Dtos;
using Apps.Document360.Models.Polling;
using Apps.Document360.Models.Responses;
using Apps.Document360.Providers;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;

namespace Apps.Document360.Events;


[PollingEventList("Articles")]
public class ArticlePollingList(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [PollingEvent("On articles created",
        Description = "Triggered on an interval and outputs the articles created since the previous check.")]
    public async Task<PollingEventResponse<PollingMemory, ArticlesEventResponse>> OnArticlesCreated(
        PollingEventRequest<PollingMemory> request,
        [PollingEventParameter] ArticlesCreatedFilter filter)
    {
        Validate.Workspace(filter);


        if (request.Memory?.LastPollingTime is null)
            return Respond(new PollingMemory { LastPollingTime = DateTime.UtcNow }, null, filter.WorkspaceId);

        var articles = await GetArticlesAsync(filter.WorkspaceId, filter.Locale, filter.Visibility);


        var memory = new PollingMemory { LastPollingTime = DateTime.UtcNow };

        var created = ArticleFilters.CreatedAfter(articles, request.Memory.LastPollingTime.Value);
        created = ArticleFilters.ByPublicationStatus(created, filter.PublicationStatus);
        created = await KeepTaggedAsync(created, filter.Locale, filter.Tags);

        return Respond(memory, created, filter.WorkspaceId);
    }

    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdatedMultiple)]
    [PollingEvent("On articles published",
        Description =
            "Triggered on an interval and outputs the articles that went live, or that had a newer version put " +
            "live, since the previous check.")]
    public async Task<PollingEventResponse<PublishedVersionsMemory, ArticlesEventResponse>> OnArticlesPublished(
        PollingEventRequest<PublishedVersionsMemory> request,
        [PollingEventParameter] ArticlesPublishedFilter filter)
    {
        Validate.Workspace(filter);

        var articles = await GetArticlesAsync(filter.WorkspaceId, filter.Locale, filter.Visibility);

        var memory = new PublishedVersionsMemory
        {
            LastPollingTime = DateTime.UtcNow,
            PublishedVersions = SnapshotLiveVersions(articles)
        };


        if (request.Memory?.PublishedVersions is null) return Respond(memory, null, filter.WorkspaceId);

        var published = KeepNewlyLive(articles, request.Memory.PublishedVersions);
        published = await KeepTaggedAsync(published, filter.Locale, filter.Tags);

        return Respond(memory, published, filter.WorkspaceId);
    }

    private Task<List<ArticleListItemDto>> GetArticlesAsync(string workspaceId, string? locale, string? visibility) =>
        ArticleProvider.GetArticlesAsync(Client, workspaceId, locale, Validate.Option(visibility, "Visibility"));

    private static Dictionary<string, int> SnapshotLiveVersions(IEnumerable<ArticleListItemDto> articles) =>
        articles
            .GroupBy(article => article.Id)
            .ToDictionary(group => group.Key, group => group.Max(article => article.PublicVersion ?? 0));


    private static List<ArticleListItemDto> KeepNewlyLive(List<ArticleListItemDto> articles,
        Dictionary<string, int> previouslyLive) =>
        articles
            .Where(article =>
            {
                var live = article.PublicVersion ?? 0;
                if (live == 0) return false;

                return !previouslyLive.TryGetValue(article.Id, out var before) || live > before;
            })
            .ToList();


    private async Task<List<ArticleListItemDto>> KeepTaggedAsync(List<ArticleListItemDto> articles, string? locale,
        IEnumerable<string>? tags)
    {
        var wanted = tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList() ?? [];
        if (wanted.Count == 0 || articles.Count == 0) return articles;

        var matches = new List<ArticleListItemDto>();

        foreach (var article in articles)
        {
            var langCode = string.IsNullOrWhiteSpace(locale) ? article.LanguageCode : locale;
            if (string.IsNullOrWhiteSpace(langCode)) continue;

            try
            {
                var settings = await ArticleProvider.GetArticleSettingsAsync(Client, article.Id, langCode);

                if ((settings.Tags ?? []).Any(tag => wanted.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                    matches.Add(article);
            }
            catch (Exception ex)
            {
                InvocationContext.Logger?.LogError(
                    $"[Document360] Could not read the tags of article {article.Id}: {ex.Message}", []);
            }
        }

        return matches;
    }


    private static PollingEventResponse<TMemory, ArticlesEventResponse> Respond<TMemory>(TMemory memory,
        List<ArticleListItemDto>? articles, string workspaceId) where TMemory : PollingMemory
    {
        if (articles is not { Count: > 0 })
            return new PollingEventResponse<TMemory, ArticlesEventResponse> { FlyBird = false, Memory = memory };

        return new PollingEventResponse<TMemory, ArticlesEventResponse>
        {
            FlyBird = true,
            Memory = memory,
            Result = new ArticlesEventResponse
            {
                Items = articles.Select(article => new ArticleListItemResponse(article, workspaceId)).ToList(),
                TotalCount = articles.Count
            }
        };
    }
}
