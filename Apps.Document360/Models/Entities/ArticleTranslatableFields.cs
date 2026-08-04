namespace Apps.Document360.Models.Entities;

/// <summary>
/// The fields of an article translation that are worth translating. This is what the download side writes into the
/// file and the upload side reads back out of it.
/// </summary>
public class ArticleTranslatableFields
{
    public string ArticleId { get; set; } = string.Empty;

    public string? Title { get; set; }

    /// <summary>The article body, either markdown or HTML depending on the editor format.</summary>
    public string? Body { get; set; }

    /// <summary>The editor format the body is written in, as Document360's numeric content type.</summary>
    public int? EditorFormat { get; set; }

    public string? Description { get; set; }

    public string? SeoTitle { get; set; }

    public string? MetaDescription { get; set; }
}
