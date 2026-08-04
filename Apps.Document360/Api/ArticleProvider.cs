using Apps.Document360.Models.Dtos;
using RestSharp;

namespace Apps.Document360.Api;


public static class ArticleProvider
{
    private const int MaxPages = 50;


    public static Task<List<ArticleListItemDto>> GetArticlesAsync(Document360Client client, string workspaceId,
        string? langCode = null, int? securityVisibility = null, int maxPages = MaxPages,
        CancellationToken cancellationToken = default) =>
        client.GetPagedListAsync<ArticleListItemDto>(page =>
        {
            var request = new RestRequest($"/v2/ProjectVersions/{workspaceId}/articles", Method.Get)
                .AddQueryParameter("page", page.ToString())
                .AddQueryParameter("hitsPerPage", Document360Client.MaxPageSize.ToString());

            if (!string.IsNullOrWhiteSpace(langCode)) request.AddQueryParameter("langCode", langCode);
            if (securityVisibility.HasValue)
                request.AddQueryParameter("securityVisibility", securityVisibility.Value.ToString());

            return request;
        }, maxPages, cancellationToken);


    public static Task<ArticleSettingsDto> GetArticleSettingsAsync(Document360Client client, string articleId,
        string langCode, CancellationToken cancellationToken = default) =>
        client.GetDataAsync<ArticleSettingsDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/settings", Method.Get), cancellationToken);


    public static Task<List<ArticleVersionDto>> GetArticleVersionsAsync(Document360Client client, string articleId,
        string langCode, CancellationToken cancellationToken = default) =>
        client.GetListAsync<ArticleVersionDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/versions", Method.Get), cancellationToken);
}
