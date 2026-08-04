using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Dtos;

namespace Apps.Document360.Utils;


public static class ArticleFilters
{


    public static List<ArticleListItemDto> ByPublicationStatus(List<ArticleListItemDto> articles,
        string? publicationStatus) => publicationStatus switch
    {
        PublicationStatusDataHandler.Published => articles.Where(x => (x.PublicVersion ?? 0) > 0).ToList(),
        PublicationStatusDataHandler.Draft => articles.Where(x => (x.PublicVersion ?? 0) == 0).ToList(),
        _ => articles
    };


    public static List<ArticleListItemDto> ModifiedAfter(List<ArticleListItemDto> articles, DateTime threshold)
    {
        var since = threshold.ToUniversalTime();

        return articles
            .Where(x => x.ModifiedAt.HasValue && x.ModifiedAt.Value.ToUniversalTime() > since)
            .ToList();
    }


    public static List<ArticleListItemDto> CreatedAfter(List<ArticleListItemDto> articles, DateTime threshold)
    {
        var since = threshold.ToUniversalTime();

        return articles
            .Where(x => x.CreatedAt.HasValue && x.CreatedAt.Value.ToUniversalTime() > since)
            .ToList();
    }
}
