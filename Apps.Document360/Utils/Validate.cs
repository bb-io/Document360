using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Document360.Utils;

public static class Validate
{
    public static string Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException($"'{fieldName}' cannot be empty. Please fill it in.");

        return value;
    }

    public static string Workspace(WorkspaceOptionalIdentifier optionalIdentifier) =>
        Required(optionalIdentifier.WorkspaceId, "Workspace ID");

    public static void Article(ArticleOptionalIdentifier optionalIdentifier)
    {
        Required(optionalIdentifier.ArticleId, "Article ID");
        Required(optionalIdentifier.Locale, "Locale");
    }

    public static int? Option(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (!int.TryParse(value, out var parsed))
            throw new PluginMisconfigurationException(
                $"'{value}' is not a valid choice for '{fieldName}'. Please pick one of the offered options.");

        return parsed;
    }
}
