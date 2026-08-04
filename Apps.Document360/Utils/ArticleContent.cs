using System.Text;
using Apps.Document360.Models.Dtos;

namespace Apps.Document360.Utils;


public static class ArticleContent
{
    public static string Select(ArticleDto article) =>
        article.ContentType == Document360Enums.EditorFormatMarkdown
            ? FirstNonEmpty(article.Content, article.HtmlContent)
            : FirstNonEmpty(article.HtmlContent, article.Content);


    public static string DecodeUtf8(byte[] bytes)
    {
        var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        return new UTF8Encoding(false).GetString(bytes, hasBom ? 3 : 0, bytes.Length - (hasBom ? 3 : 0));
    }

    private static string FirstNonEmpty(string? first, string? second) =>
        !string.IsNullOrEmpty(first) ? first : second ?? string.Empty;
}
