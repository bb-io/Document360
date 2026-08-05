namespace Apps.Document360.Models.Entities;


public class ArticleTranslatableFields
{
    public string ArticleId { get; set; } = string.Empty;

    public string? Title { get; set; }


    public string? Body { get; set; }


    public int? EditorFormat { get; set; }

    public string? Description { get; set; }

    public string? SeoTitle { get; set; }

    public string? MetaDescription { get; set; }
}
