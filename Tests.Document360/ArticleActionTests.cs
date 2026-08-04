using Apps.Document360.Actions;
using Apps.Document360.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Document360.Base;

namespace Tests.Document360;

[TestClass]
public class ArticleActionTests : TestBase
{
    private ArticleActions Actions => new(InvocationContext, FileManager);

    [TestMethod]
    public async Task CreateDraftArticle_MarkdownFile_CreatesDraft()
    {
        var result = await Actions.CreateDraftArticle(new CreateDraftArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            CategoryId = TestData.CategoryId,
            UserId = TestData.UserId,
            Title = $"Blackbird integration test {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
            File = new FileReference { Name = TestData.SampleMarkdownFileName, ContentType = "text/markdown" }
        });

        Console.WriteLine($"Created {result.ArticleId} ({result.Status}) at {result.ArticleUrl}");

        Assert.IsFalse(string.IsNullOrWhiteSpace(result.ArticleId));
        Assert.AreEqual("Draft", result.Status);
        Assert.AreEqual("Markdown", result.EditorFormat);
        Assert.AreEqual(0, result.PublishedVersion);
        Assert.AreEqual(TestData.WorkspaceId, result.WorkspaceId);
    }

    [TestMethod]
    public async Task UpdateArticle_TranslationFromFile_UpdatesTranslation()
    {
        var result = await Actions.UpdateArticle(new UpdateArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.TargetLocale,
            Title = $"Blackbird integration test translation {DateTime.UtcNow:HH:mm:ss}",
            File = new FileReference { Name = TestData.SampleMarkdownFileName, ContentType = "text/markdown" },
            TranslationStatus = "2",
            UserId = TestData.UserId,
            Tags = ["localization", "blackbird-test"]
        });

        Console.WriteLine(
            $"Updated version {result.VersionNumber} (copied to new draft: {result.WasCopiedToNewDraft})");
        Console.WriteLine($"Tags: {string.Join(", ", result.Tags)}");

        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
        Assert.AreEqual("Draft", result.Status);
        Assert.AreEqual("Translated", result.TranslationStatus);
        Assert.IsTrue(result.Content.Contains("Sample body used by the Document360 integration tests"));
        Assert.IsTrue(result.Tags.Contains("blackbird-test"));
    }

    [TestMethod]
    public async Task UpdateArticle_TagsOnly_KeepsContent()
    {
        var before = await Actions.GetArticleInfo(new GetArticleInfoRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.TargetLocale
        });

        var result = await Actions.UpdateArticle(new UpdateArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.TargetLocale,
            Tags = ["localization", "blackbird-test"]
        });

        Console.WriteLine($"Tags after update: {string.Join(", ", result.Tags)}");

        Assert.AreEqual(before.Title, result.Title);
        Assert.AreEqual(before.Content, result.Content);
        Assert.IsFalse(result.WasCopiedToNewDraft, "Writing only tags must not create a new version.");
    }

    [TestMethod]
    public async Task PublishArticle_NoVersionGiven_PublishesLatest()
    {
        var result = await Actions.PublishArticle(new PublishArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.SourceLocale,
            UserId = TestData.UserId,
            PublishMessage = "Published by the Blackbird integration tests."
        });

        Console.WriteLine($"Published version {result.PublishedVersion} at {result.ArticleUrl} ({result.Status}), " +
                          $"already live: {result.WasAlreadyLive}");


        Assert.IsFalse(string.IsNullOrWhiteSpace(result.ArticleUrl));
        Assert.IsTrue(result.PublishedVersion > 0);
        Assert.AreEqual("Published", result.Status);
    }


    [TestMethod]
    public async Task GetArticleInfo_ExistingTranslation_ReturnsMetadata()
    {
        var result = await Actions.GetArticleInfo(new GetArticleInfoRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.TargetLocale
        });

        Console.WriteLine($"{result.Title} | {result.Status} | tags: {string.Join(", ", result.Tags)}");
        Console.WriteLine($"Available locales: {string.Join(", ", result.AvailableLocales.Select(x => x.Locale))}");

        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
        Assert.AreEqual(TestData.WorkspaceId, result.WorkspaceId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Title));
        Assert.IsNotNull(result.Tags);
        Assert.IsNotNull(result.CustomFields);
        Assert.IsTrue(result.AvailableLocales.Count > 0);
    }

    [TestMethod]
    public async Task SearchArticles_ByLocale_ReturnsAllPages()
    {
        var result = await Actions.SearchArticles(new SearchArticlesRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            Locale = TestData.TargetLocale
        });

        Console.WriteLine($"Found {result.TotalCount} articles");

        Assert.IsNotNull(result.Articles);
        Assert.AreEqual(result.Articles.Count, result.TotalCount);
        Assert.IsTrue(result.TotalCount > 100, "Pagination should return more than a single page of 100 articles.");
        Assert.AreEqual(result.Articles.Count, result.Articles.Select(x => x.ContentId).Distinct().Count());
    }

    [TestMethod]
    public async Task SearchArticles_NeedsTranslation_ReturnsSubset()
    {
        var all = await Actions.SearchArticles(new SearchArticlesRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            Locale = TestData.TargetLocale
        });

        var needsTranslation = await Actions.SearchArticles(new SearchArticlesRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            Locale = TestData.TargetLocale,
            TranslationStatus = "1"
        });

        Console.WriteLine($"{needsTranslation.TotalCount} of {all.TotalCount} articles need translation");

        Assert.IsNotNull(needsTranslation.Articles);
        Assert.IsTrue(needsTranslation.TotalCount <= all.TotalCount);
    }

    [TestMethod]
    public async Task SearchArticles_PublishedOnly_ReturnsLiveArticles()
    {
        var result = await Actions.SearchArticles(new SearchArticlesRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            Locale = TestData.SourceLocale,
            PublicationStatus = "published",
            MaxResults = 5
        });

        Console.WriteLine($"Found {result.TotalCount} published articles");

        Assert.IsNotNull(result.Articles);
        Assert.IsTrue(result.TotalCount <= 5);
        Assert.IsTrue(result.Articles.All(x => x.PublishedVersion > 0));
    }

    [TestMethod]
    public async Task SearchArticles_NoWorkspace_Fails()
    {
        await Assert.ThrowsExceptionAsync<Blackbird.Applications.Sdk.Common.Exceptions.
            PluginMisconfigurationException>(
            () => Actions.SearchArticles(new SearchArticlesRequest { WorkspaceId = string.Empty }));
    }
}
