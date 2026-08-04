using System.Text;
using System.Text.RegularExpressions;
using Apps.Document360.Constants;
using Apps.Document360.Models.Entities;
using HtmlAgilityPack;

namespace Apps.Document360.Utils;

/// <summary>
/// Reads a translated file back into article fields. It reverses <see cref="ArticleHtmlBuilder"/>: the structure of
/// the article comes from the attributes in the file, never from guesswork, and anything nobody translated is
/// restored from the untranslated value the file carries.
/// </summary>
public static partial class ArticleHtmlReader
{
    public static ArticleTranslatableFields Read(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var container = document.DocumentNode
                            .SelectSingleNode($"//*[@{ArticleContentAttributes.ArticleId}]") ??
                        document.DocumentNode;

        var fields = new ArticleTranslatableFields { ArticleId = ReadArticleId(document, container) };

        // Only nodes that say both which field they hold and how to read it are ours.
        var fieldNodes = container.SelectNodes(
            $".//*[@{ArticleContentAttributes.FieldId}][@{ArticleContentAttributes.FieldType}]")?.ToList() ?? [];

        foreach (var node in fieldNodes)
        {
            var fieldId = node.GetAttributeValue(ArticleContentAttributes.FieldId, string.Empty);
            var fieldType = node.GetAttributeValue(ArticleContentAttributes.FieldType, string.Empty);

            switch (fieldId)
            {
                case ArticleContentAttributes.FieldIds.Title:
                    fields.Title = ReadText(node);
                    break;
                case ArticleContentAttributes.FieldIds.Description:
                    fields.Description = ReadText(node);
                    break;
                case ArticleContentAttributes.FieldIds.SeoTitle:
                    fields.SeoTitle = ReadText(node);
                    break;
                case ArticleContentAttributes.FieldIds.MetaDescription:
                    fields.MetaDescription = ReadText(node);
                    break;
                case ArticleContentAttributes.FieldIds.Content:
                    fields.Body = ReadBody(node, fieldType);
                    fields.EditorFormat = fieldType == ArticleContentAttributes.FieldTypes.Markdown
                        ? Document360Enums.EditorFormatMarkdown
                        : Document360Enums.EditorFormatAdvancedWysiwyg;
                    break;
            }
        }

        return fields;
    }

    /// <summary>Collapses runs of whitespace, so a body is not rewritten just because a tool reflowed the markup.</summary>
    public static bool IsSameContent(string? left, string? right) =>
        Whitespace().Replace(left ?? string.Empty, " ").Trim() ==
        Whitespace().Replace(right ?? string.Empty, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private static string ReadArticleId(HtmlDocument document, HtmlNode container)
    {
        // The article id lives in blackbird-ucid; the container attribute is the fallback for hand-made files.
        var ucid = document.DocumentNode
            .SelectSingleNode("//meta[@name='blackbird-ucid']")?
            .GetAttributeValue("content", string.Empty);

        return string.IsNullOrWhiteSpace(ucid)
            ? container.GetAttributeValue(ArticleContentAttributes.ArticleId, string.Empty)
            : ucid;
    }

    private static string ReadText(HtmlNode node) =>
        (HtmlEntity.DeEntitize(node.InnerText ?? string.Empty) ?? string.Empty).Trim();

    private static string ReadBody(HtmlNode node, string fieldType)
    {
        var source = ReadSourceValue(node);

        return fieldType == ArticleContentAttributes.FieldTypes.Markdown
            ? ReadMarkdownBody(node, source)
            : ReadHtmlBody(node, source);
    }

    /// <summary>
    /// Rebuilds the markdown from its translated lines, laid back over the untranslated body so blank lines and
    /// indentation stay exactly as they were. A line nobody touched is restored character for character.
    /// </summary>
    private static string ReadMarkdownBody(HtmlNode node, string? source)
    {
        var lineNodes = node.SelectNodes($".//*[@{ArticleContentAttributes.LineIndex}]")?.ToList() ?? [];

        if (source is null)
            return string.Join("\n", lineNodes.Select(ReadText));

        var sourceLines = source.Replace("\r\n", "\n").Split('\n');
        var lines = (string[])sourceLines.Clone();

        foreach (var lineNode in lineNodes)
        {
            var index = lineNode.GetAttributeValue(ArticleContentAttributes.LineIndex, -1);
            if (index < 0 || index >= lines.Length) continue;

            var translated = ReadText(lineNode);
            if (translated == sourceLines[index].Trim()) continue;

            var indentation = sourceLines[index][..(sourceLines[index].Length - sourceLines[index].TrimStart().Length)];
            lines[index] = indentation + translated;
        }

        return string.Join(source.Contains("\r\n") ? "\r\n" : "\n", lines);
    }

    private static string ReadHtmlBody(HtmlNode node, string? source)
    {
        var translated = node.InnerHtml.Trim();

        // Nothing was translated, so hand back the markup Document360 gave us rather than a reformatted copy.
        return source is not null && IsSameContent(translated, source) ? source : translated;
    }

    private static string? ReadSourceValue(HtmlNode node)
    {
        var encoded = node.GetAttributeValue(ArticleContentAttributes.SourceValue, string.Empty);
        if (string.IsNullOrWhiteSpace(encoded)) return null;

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
