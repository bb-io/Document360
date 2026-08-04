using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Document360.Utils;

public static class Validate
{
    /// <returns>The value, so a caller can use it without a null check of its own.</returns>
    public static string Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException($"'{fieldName}' cannot be empty. Please fill it in.");

        return value;
    }

    /// <summary>Reads the workspace of an action that addresses a whole workspace and cannot run without one.</summary>
    public static string Workspace(WorkspaceOptionalIdentifier optionalIdentifier) =>
        Required(optionalIdentifier.WorkspaceId, "Workspace ID");

    /// <summary>
    /// The inputs needed to address one article translation. The workspace is deliberately not among them: these
    /// endpoints are addressed by article ID alone, and Document360 reports the workspace back on the article.
    /// </summary>
    public static void Article(ArticleOptionalIdentifier optionalIdentifier)
    {
        Required(optionalIdentifier.ArticleId, "Article ID");
        Required(optionalIdentifier.Locale, "Locale");
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
