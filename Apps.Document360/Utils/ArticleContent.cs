using System.Text;
using Apps.Document360.Models.Dtos;

namespace Apps.Document360.Utils;

/// <summary>
/// Document360 keeps a markdown article's body in <c>content</c> and an advanced WYSIWYG article's body in
/// <c>html_content</c>. These helpers hide that asymmetry so the rest of the app deals with one body.
/// </summary>
public static class ArticleContent
{
    public static string Select(ArticleDto article) =>
        article.ContentType == Document360Enums.EditorFormatMarkdown
            ? FirstNonEmpty(article.Content, article.HtmlContent)
            : FirstNonEmpty(article.HtmlContent, article.Content);

    /// <summary>Reads a Blackbird file as UTF-8 text, dropping a byte order mark if one is present.</summary>
    public static string DecodeUtf8(byte[] bytes)
    {
        var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        return new UTF8Encoding(false).GetString(bytes, hasBom ? 3 : 0, bytes.Length - (hasBom ? 3 : 0));
    }

    private static string FirstNonEmpty(string? first, string? second) =>
        !string.IsNullOrEmpty(first) ? first : second ?? string.Empty;
}
