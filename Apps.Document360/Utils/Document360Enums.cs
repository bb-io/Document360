namespace Apps.Document360.Utils;


public static class Document360Enums
{
    public const int EditorFormatMarkdown = 0;
    public const int EditorFormatAdvancedWysiwyg = 2;

    public const int ArticleStatusDraft = 0;

    public static string ArticleStatus(int? value) => value switch
    {
        null => string.Empty,
        0 => "Draft",
        3 => "Published",
        10 => "Unpublished",
        _ => $"Status {value}"
    };

    public static string EditorFormat(int? value) => value switch
    {
        null => string.Empty,
        0 => "Markdown",
        1 or 2 => "Advanced WYSIWYG",
        _ => $"Format {value}"
    };

    public static string TranslationStatus(int? value) => value switch
    {
        null => string.Empty,
        0 => "None",
        1 => "Needs translation",
        2 => "Translated",
        3 => "In progress",
        _ => $"Translation status {value}"
    };

    public static string Visibility(int? value) => value switch
    {
        null => string.Empty,
        0 => "Public",
        1 => "Protected",
        _ => $"Visibility {value}"
    };

    public static string CustomFieldType(int? value) => value switch
    {
        null => string.Empty,
        0 => "Text",
        1 => "Text area",
        2 => "Dropdown",
        3 => "Multi-select dropdown",
        4 => "Date",
        5 => "Boolean",
        6 => "Number",
        _ => $"Type {value}"
    };

    public static string WorkspaceType(int? value) => value switch
    {
        null => string.Empty,
        0 => "Documentation",
        1 => "API documentation",
        _ => $"Type {value}"
    };
}
