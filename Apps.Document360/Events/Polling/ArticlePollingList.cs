using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Dtos;
using Apps.Document360.Models.Polling;
using Apps.Document360.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Apps.Document360.Events.Polling;

/// <summary>
/// Document360 has no webhooks for article changes, so both events poll the workspace's article list and compare
/// it against what the previous run saw.
/// </summary>
[PollingEventList("Articles")]
public class ArticlePollingList(InvocationContext invocationContext) : Invocable(invocationContext)
{
    [PollingEvent("On article created",
        Description = "Triggered on an interval and outputs the articles created since the previous check.")]
    public async Task<PollingEventResponse<ArticleCreatedMemory, ArticlesEventResponse>> OnArticleCreated(
        PollingEventRequest<ArticleCreatedMemory> request,
        [PollingEventParameter] ArticleCreatedFilter filter)
    {
        RequireWorkspace(filter.WorkspaceId);

        // On the very first run there is nothing to compare against, so only the starting point is remembered.
        if (request.Memory?.LastPollingTime is null) return CreatedBaseline();

        var since = request.Memory.LastPollingTime.Value.ToUniversalTime();
        var articles = await FetchArticlesAsync(filter.WorkspaceId, filter.Locale, filter.Visibility);

        var created = articles
            .Where(article => article.CreatedAt.HasValue && article.CreatedAt.Value.ToUniversalTime() > since)
            .ToList();

        created = ApplyPublicationStatus(created, filter.PublicationStatus);
        created = await ApplyTagFilterAsync(created, filter.Locale, filter.Tags);

        var memory = new ArticleCreatedMemory { LastPollingTime = DateTime.UtcNow };

        return created.Count == 0
            ? new PollingEventResponse<ArticleCreatedMemory, ArticlesEventResponse>
            {
                FlyBird = false,
                Memory = memory,
                Result = null
            }
            : new PollingEventResponse<ArticleCreatedMemory, ArticlesEventResponse>
            {
                FlyBird = true,
                Memory = memory,
                Result = BuildResult(created, filter.WorkspaceId)
            };
    }

    [PollingEvent("On article published",
        Description =
            "Triggered on an interval and outputs the articles that went live, or that had a newer version put " +
            "live, since the previous check.")]
    public async Task<PollingEventResponse<ArticlePublishedMemory, ArticlesEventResponse>> OnArticlePublished(
        PollingEventRequest<ArticlePublishedMemory> request,
        [PollingEventParameter] ArticlePublishedFilter filter)
    {
        RequireWorkspace(filter.WorkspaceId);

        var articles = await FetchArticlesAsync(filter.WorkspaceId, filter.Locale, filter.Visibility);
        var liveVersions = SnapshotLiveVersions(articles);

        // The first run records which version of each article is live without triggering anything.
        if (request.Memory?.PublishedVersions is null)
            return new PollingEventResponse<ArticlePublishedMemory, ArticlesEventResponse>
            {
                FlyBird = false,
                Memory = new ArticlePublishedMemory
                {
                    LastPollingTime = DateTime.UtcNow,
                    PublishedVersions = liveVersions
                },
                Result = null
            };

        var previous = request.Memory.PublishedVersions;

        var published = articles
            .Where(article =>
            {
                var live = article.PublicVersion ?? 0;
                if (live == 0) return false;
                return !previous.TryGetValue(article.Id, out var previouslyLive) || live > previouslyLive;
            })
            .ToList();

        published = await ApplyTagFilterAsync(published, filter.Locale, filter.Tags);

        var memory = new ArticlePublishedMemory
        {
            LastPollingTime = DateTime.UtcNow,
            PublishedVersions = liveVersions
        };

        return published.Count == 0
            ? new PollingEventResponse<ArticlePublishedMemory, ArticlesEventResponse>
            {
                FlyBird = false,
                Memory = memory,
                Result = null
            }
            : new PollingEventResponse<ArticlePublishedMemory, ArticlesEventResponse>
            {
                FlyBird = true,
                Memory = memory,
                Result = BuildResult(published, filter.WorkspaceId)
            };
    }

    #region Helpers

    private Task<List<ArticleListItemDto>> FetchArticlesAsync(string workspaceId, string? locale, string? visibility)
    {
        var securityVisibility = string.IsNullOrWhiteSpace(visibility) || !int.TryParse(visibility, out var parsed)
            ? (int?)null
            : parsed;

        return Articles.GetArticlesAsync(workspaceId, locale, securityVisibility);
    }

    private static Dictionary<string, int> SnapshotLiveVersions(IEnumerable<ArticleListItemDto> articles) =>
        articles
            .GroupBy(article => article.Id)
            .ToDictionary(group => group.Key, group => group.Max(article => article.PublicVersion ?? 0));

    private static List<ArticleListItemDto> ApplyPublicationStatus(List<ArticleListItemDto> articles,
        string? publicationStatus) => publicationStatus switch
    {
        PublicationStatusDataHandler.Published => articles.Where(x => (x.PublicVersion ?? 0) > 0).ToList(),
        PublicationStatusDataHandler.Draft => articles.Where(x => (x.PublicVersion ?? 0) == 0).ToList(),
        _ => articles
    };

    /// <summary>
    /// Tags are not part of the article list, so each candidate needs one extra call. That is acceptable here
    /// because a poll only ever looks at the handful of articles that changed.
    /// </summary>
    private async Task<List<ArticleListItemDto>> ApplyTagFilterAsync(List<ArticleListItemDto> articles,
        string? locale, IEnumerable<string>? tags)
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
                var settings = await Articles.GetArticleSettingsAsync(article.Id, langCode);
                var articleTags = settings.Tags ?? [];

                if (articleTags.Any(tag => wanted.Contains(tag, StringComparer.OrdinalIgnoreCase)))
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

    private static ArticlesEventResponse BuildResult(List<ArticleListItemDto> articles, string workspaceId) =>
        new()
        {
            Articles = articles.Select(article => new ArticleListItemResponse(article, workspaceId)).ToList(),
            TotalCount = articles.Count
        };

    private static PollingEventResponse<ArticleCreatedMemory, ArticlesEventResponse> CreatedBaseline() =>
        new()
        {
            FlyBird = false,
            Memory = new ArticleCreatedMemory { LastPollingTime = DateTime.UtcNow },
            Result = null
        };

    private static void RequireWorkspace(string workspaceId)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
            throw new PluginMisconfigurationException("'Workspace ID' cannot be empty. Please fill it in.");
    }

    #endregion
}
