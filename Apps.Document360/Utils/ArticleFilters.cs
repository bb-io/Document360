using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Dtos;

namespace Apps.Document360.Utils;

/// <summary>
/// The filters 'Search articles' and the article polling events both apply to a workspace's article list.
/// Document360 offers no server-side filtering for these, so the list is narrowed here.
/// </summary>
public static class ArticleFilters
{
    /// <summary>
    /// An article is live when it has a published version. Its status only describes the newest version, so an
    /// article that is live and has a newer draft still reports "Draft" and cannot be used for this.
    /// </summary>
    public static List<ArticleListItemDto> ByPublicationStatus(List<ArticleListItemDto> articles,
        string? publicationStatus) => publicationStatus switch
    {
        PublicationStatusDataHandler.Published => articles.Where(x => (x.PublicVersion ?? 0) > 0).ToList(),
        PublicationStatusDataHandler.Draft => articles.Where(x => (x.PublicVersion ?? 0) == 0).ToList(),
        _ => articles
    };

    /// <summary>Keeps the articles changed after the given moment, comparing in UTC.</summary>
    public static List<ArticleListItemDto> ModifiedAfter(List<ArticleListItemDto> articles, DateTime threshold)
    {
        var since = threshold.ToUniversalTime();

        return articles
            .Where(x => x.ModifiedAt.HasValue && x.ModifiedAt.Value.ToUniversalTime() > since)
            .ToList();
    }

    /// <summary>Keeps the articles created after the given moment, comparing in UTC.</summary>
    public static List<ArticleListItemDto> CreatedAfter(List<ArticleListItemDto> articles, DateTime threshold)
    {
        var since = threshold.ToUniversalTime();

        return articles
            .Where(x => x.CreatedAt.HasValue && x.CreatedAt.Value.ToUniversalTime() > since)
            .ToList();
    }
}
