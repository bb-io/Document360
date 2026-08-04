using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Document360.Utils;

/// <summary>
/// The input checks the actions and the polling events share. Document360 answers a missing identifier with a
/// generic failure, so an empty input is caught here and named, before a request goes out.
/// </summary>
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

    /// <summary>
    /// Reads back one of the numeric options a dropdown offers. A value that is not a number can only come from a
    /// hand-typed or mapped input, which is worth naming rather than silently ignoring.
    /// </summary>
    public static int? Option(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (!int.TryParse(value, out var parsed))
            throw new PluginMisconfigurationException(
                $"'{value}' is not a valid choice for '{fieldName}'. Please pick one of the offered options.");

        return parsed;
    }
}
