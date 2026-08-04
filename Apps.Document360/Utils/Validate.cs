using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Document360.Utils;


public static class Validate
{
    public static void Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException($"'{fieldName}' cannot be empty. Please fill it in.");
    }

    public static void Workspace(WorkspaceIdentifier identifier) =>
        Required(identifier.WorkspaceId, "Workspace ID");

    public static void Article(ArticleIdentifier identifier)
    {
        Workspace(identifier);
        Required(identifier.ArticleId, "Article ID");
        Required(identifier.Locale, "Locale");
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
