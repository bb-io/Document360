using System.Text;
using Apps.Document360.Constants;
using Apps.Document360.Models.Entities;
using Blackbird.Filters.Coders;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Shared;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Apps.Document360.Utils;

public static class ArticleHtmlBuilder
{
    private const int DescriptionMaxLength = 250;

    public static string Build(ArticleTranslatableFields fields, string locale, string fileName, string? publicUrl,
        string systemRef, string? revisedBy)
    {
        var document = new HtmlDocument();

        var html = document.CreateElement("html");
        document.DocumentNode.AppendChild(html);

        var body = document.CreateElement("body");
        html.AppendChild(body);


        body.SetAttributeValue("its-rev-tool", ArticleContentAttributes.SystemName);
        body.SetAttributeValue("its-rev-tool-ref", systemRef);
        if (!string.IsNullOrWhiteSpace(revisedBy)) body.SetAttributeValue("its-rev-person", revisedBy);

        var container = document.CreateElement("div");
        container.SetAttributeValue(ArticleContentAttributes.ArticleId, fields.ArticleId);
        body.AppendChild(container);

        AppendTextField(document, container, fields.ArticleId, ArticleContentAttributes.FieldIds.Title, fields.Title,
            "h1");
        AppendBodyField(document, container, fields);
        AppendTextField(document, container, fields.ArticleId, ArticleContentAttributes.FieldIds.Description,
            fields.Description, "div", DescriptionMaxLength);
        AppendTextField(document, container, fields.ArticleId, ArticleContentAttributes.FieldIds.SeoTitle,
            fields.SeoTitle, "div");
        AppendTextField(document, container, fields.ArticleId, ArticleContentAttributes.FieldIds.MetaDescription,
            fields.MetaDescription, "div");


        var coder = new HtmlCoder();
        var content = coder.Deserialize(document.DocumentNode.OuterHtml, fileName);

        content.Language = locale;
        content.SystemReference = new SystemReference
        {
            ContentId = fields.ArticleId,
            ContentName = fields.Title ?? fields.ArticleId,
            PublicUrl = publicUrl,
            SystemName = ArticleContentAttributes.SystemName,
            SystemRef = systemRef
        };

        return coder.Serialize(content, MetadataHandling.Include);
    }

    public static string BuildFileName(string? title, string articleId, string locale)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var stem = new string((title ?? string.Empty).Select(c => invalid.Contains(c) ? '-' : c).ToArray()).Trim();

        return $"{(string.IsNullOrEmpty(stem) ? articleId : stem)}_{locale}.html";
    }

    private static void AppendTextField(HtmlDocument document, HtmlNode container, string articleId, string fieldId,
        string? value, string tag, int? maximumSize = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var node = document.CreateElement(tag);
        node.InnerHtml = HtmlEntity.Entitize(value, false, true) ?? string.Empty;

        Describe(node, articleId, fieldId, ArticleContentAttributes.FieldTypes.Text, maximumSize);
        container.AppendChild(node);
    }

    private static void AppendBodyField(HtmlDocument document, HtmlNode container, ArticleTranslatableFields fields)
    {
        if (string.IsNullOrWhiteSpace(fields.Body)) return;

        var isMarkdown = fields.EditorFormat == Document360Enums.EditorFormatMarkdown;
        var node = document.CreateElement("div");

        Describe(node, fields.ArticleId, ArticleContentAttributes.FieldIds.Content,
            isMarkdown ? ArticleContentAttributes.FieldTypes.Markdown : ArticleContentAttributes.FieldTypes.Html);


        node.SetAttributeValue(ArticleContentAttributes.SourceValue, Encode(fields.Body));

        if (isMarkdown)
            AppendMarkdownLines(document, node, fields);
        else
            node.InnerHtml = fields.Body;

        container.AppendChild(node);
    }


    private static void AppendMarkdownLines(HtmlDocument document, HtmlNode node, ArticleTranslatableFields fields)
    {
        var lines = SplitLines(fields.Body!);
        node.SetAttributeValue(ArticleContentAttributes.LineCount, lines.Length.ToString());

        for (var index = 0; index < lines.Length; index++)
        {
            var text = lines[index].Trim();
            if (text.Length == 0) continue;

            var lineNode = document.CreateElement("p");
            lineNode.InnerHtml = HtmlEntity.Entitize(text, false, true) ?? string.Empty;
            lineNode.SetAttributeValue(ArticleContentAttributes.LineIndex, index.ToString());
            lineNode.SetAttributeValue(ArticleContentAttributes.BlackbirdKey,
                $"{fields.ArticleId}-{ArticleContentAttributes.FieldIds.Content}-{index}");

            node.AppendChild(lineNode);
        }
    }

    private static void Describe(HtmlNode node, string articleId, string fieldId, string fieldType,
        int? maximumSize = null)
    {
        node.SetAttributeValue(ArticleContentAttributes.FieldId, fieldId);
        node.SetAttributeValue(ArticleContentAttributes.FieldType, fieldType);
        node.SetAttributeValue(ArticleContentAttributes.BlackbirdKey, $"{articleId}-{fieldId}");

        if (maximumSize.HasValue)
            node.SetAttributeValue(ArticleContentAttributes.BlackbirdSize,
                JsonConvert.SerializeObject(new SizeRestrictions { MaximumSize = maximumSize },
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
    }

    private static string[] SplitLines(string value) => value.Replace("\r\n", "\n").Split('\n');

    private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
}
