namespace Tests.Document360.Base;

/// <summary>
/// Fixtures of the sandbox project these integration tests run against. Replace them when pointing the tests at
/// another Document360 project.
/// </summary>
public static class TestData
{
    /// <summary>Workspace "Cameras and Video".</summary>
    public const string WorkspaceId = "c5bbf59f-0e9f-4c23-8135-b8488167c50f";

    /// <summary>Category "Test Category" in the workspace above.</summary>
    public const string CategoryId = "fab179e2-7378-4b0e-9519-1521fe9674e6";

    /// <summary>A team account Document360 accepts as an article author.</summary>
    public const string UserId = "64d5e52b-b29d-4d5c-997b-6ee1f843527b";

    /// <summary>A markdown article that has an English source and a French translation.</summary>
    public const string MarkdownArticleId = "77857a1d-72ad-4f54-888b-c494a7501ed1";

    public const string SourceLocale = "en";

    public const string TargetLocale = "fr";

    public const string SampleMarkdownFileName = "sample-article.md";
}
