using Apps.Document360.Actions;
using Apps.Document360.Models.Requests;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Document360.Base;

namespace Tests.Document360;

/// <summary>
/// Document360 addresses an article by its ID alone, so the workspace is only needed by the actions that work on a
/// whole workspace. These tests pin down which is which, because the difference is invisible in the signatures.
/// </summary>
[TestClass]
public class WorkspaceOptionalTests : TestBase
{
    private ArticleActions Actions => new(InvocationContext, FileManager);

    [TestMethod]
    public async Task GetArticleInfo_WithoutWorkspace_Works()
    {
        var result = await Actions.GetArticleInfo(new GetArticleOptionalInfoRequest
        {
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.SourceLocale
        });

        Console.WriteLine($"{result.Title} | workspace resolved by Document360: {result.WorkspaceId}");

        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
        Assert.AreEqual(TestData.WorkspaceId, result.WorkspaceId,
            "Document360 reports the workspace back, which is why it does not have to be supplied.");
    }

    [TestMethod]
    public async Task DownloadArticle_WithoutWorkspace_Works()
    {
        var result = await Actions.DownloadArticle(
            new DownloadArticleRequest
            {
                ContentId = TestData.MarkdownArticleId,
                Locale = TestData.SourceLocale
            },
            new DownloadArticleOptions());

        Console.WriteLine($"Downloaded {result.Content.Name} for {result.ArticleId}");

        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Content.Name));
    }

    [TestMethod]
    public async Task UploadArticle_WithoutWorkspace_Works()
    {
        var downloaded = await Actions.DownloadArticle(
            new DownloadArticleRequest
            {
                ContentId = TestData.MarkdownArticleId,
                Locale = TestData.SourceLocale
            },
            new DownloadArticleOptions());

        var result = await Actions.UploadArticle(new UploadArticleRequest
        {
            Content = FileManager.CopyOutputToInput(downloaded.Content.Name),
            ContentId = TestData.MarkdownArticleId,
            Locale = TestData.SourceLocale,
            UserId = TestData.UserId
        });

        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
    }

    [TestMethod]
    public async Task PublishArticle_WithoutWorkspace_Works()
    {
        var result = await Actions.PublishArticle(new PublishArticleOptionalRequest
        {
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.SourceLocale,
            UserId = TestData.UserId
        });

        Console.WriteLine($"Published version {result.PublishedVersion}, already live: {result.WasAlreadyLive}");

        Assert.AreEqual("Published", result.Status);
        Assert.IsTrue(result.PublishedVersion > 0);
    }

    [TestMethod]
    public async Task UpdateArticle_WithoutWorkspace_Works()
    {
        var result = await Actions.UpdateArticle(new UpdateArticleOptionalRequest
        {
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.TargetLocale,
            Tags = ["localization", "blackbird-test"]
        });

        Console.WriteLine($"Updated {result.ArticleId}, tags: {string.Join(", ", result.Tags)}");

        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
    }

    [TestMethod]
    public async Task SearchArticles_WithoutWorkspace_IsRejected()
    {
        // The workspace sits in the URL of the article-list endpoint, which answers 404 without it, so the action
        // has to say what is missing instead of letting a bare "Not Found" through.
        var error = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
            Actions.SearchArticles(new SearchArticlesRequest { Locale = TestData.SourceLocale }));

        Console.WriteLine(error.Message);

        Assert.IsTrue(error.Message.Contains("Workspace ID"));
    }

    [TestMethod]
    public async Task CreateArticle_WithoutWorkspace_IsRejected()
    {
        // The workspace travels in the request body as project_version_id, so it is genuinely required here.
        var error = await Assert.ThrowsExceptionAsync<PluginMisconfigurationException>(() =>
            Actions.CreateArticle(new CreateArticleRequest
            {
                CategoryId = TestData.CategoryId,
                UserId = TestData.UserId,
                Title = "Rejected before anything is sent",
                File = new FileReference { Name = TestData.SampleMarkdownFileName, ContentType = "text/markdown" }
            }));

        Console.WriteLine(error.Message);

        Assert.IsTrue(error.Message.Contains("Workspace ID"));
    }
}
