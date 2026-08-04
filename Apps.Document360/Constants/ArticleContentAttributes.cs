namespace Apps.Document360.Constants;


public static class ArticleContentAttributes
{
    public const string SystemName = "Document360";

    public const string ArticleId = "data-document360-article-id";

    public const string FieldId = "data-document360-field-id";

    public const string FieldType = "data-document360-field-type";

    public const string SourceValue = "data-document360-source";

    public const string LineCount = "data-document360-lines";

    public const string LineIndex = "data-document360-line";
    
    public const string BlackbirdKey = "data-blackbird-key";
    
    public const string BlackbirdSize = "data-blackbird-size";

    public static class FieldIds
    {
        public const string Title = "title";
        public const string Content = "content";
        public const string Description = "description";
        public const string SeoTitle = "seo_title";
        public const string MetaDescription = "meta_description";
    }

    public static class FieldTypes
    {
        public const string Text = "text";
        public const string Markdown = "markdown";
        public const string Html = "html";
    }
}
