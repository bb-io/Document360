namespace Apps.Document360.Constants;

/// <summary>
/// The contract between "Download article" and "Upload article". Download writes these attributes; upload — and
/// any other app or the content lake — reads them back. Never change one side without the other.
/// </summary>
public static class ArticleContentAttributes
{
    public const string SystemName = "Document360";

    /// <summary>Marks the element that holds one article's fields.</summary>
    public const string ArticleId = "data-document360-article-id";

    /// <summary>The Document360 name of the field a node holds. Upload only looks at nodes that have this and the type.</summary>
    public const string FieldId = "data-document360-field-id";

    /// <summary>How the node's content has to be read back: text, markdown or HTML.</summary>
    public const string FieldType = "data-document360-field-type";

    /// <summary>
    /// The untranslated value, base64 encoded. Base64 has no characters an HTML attribute or a translation tool
    /// could alter, so blank lines, indentation and trailing spaces all survive the round trip.
    /// </summary>
    public const string SourceValue = "data-document360-source";

    /// <summary>The number of lines the untranslated body had, so blank lines can be put back.</summary>
    public const string LineCount = "data-document360-lines";

    /// <summary>The zero-based position of a line inside the body.</summary>
    public const string LineIndex = "data-document360-line";

    /// <summary>A globally unique key per translatable value, used for field-level change detection.</summary>
    public const string BlackbirdKey = "data-blackbird-key";

    /// <summary>The length limit Document360 enforces on the field, so translations do not overflow it.</summary>
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
