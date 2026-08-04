using Apps.Document360.Events.Polling;
using Apps.Document360.Models.Polling;
using Blackbird.Applications.Sdk.Common.Polling;
using Tests.Document360.Base;

namespace Tests.Document360;

[TestClass]
public class ArticlePollingTests : TestBase
{
    private ArticlePollingList Events => new(InvocationContext);

    private static ArticleCreatedFilter CreatedFilter => new()
    {
        WorkspaceId = TestData.WorkspaceId,
        Locale = TestData.SourceLocale
    };

    private static ArticlePublishedFilter PublishedFilter => new()
    {
        WorkspaceId = TestData.WorkspaceId,
        Locale = TestData.SourceLocale
    };

    [TestMethod]
    public async Task OnArticleCreated_FirstRun_DoesNotFly()
    {
        var result = await Events.OnArticleCreated(
            new PollingEventRequest<ArticleCreatedMemory>(), CreatedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, memory: {result.Memory?.LastPollingTime}");

        Assert.IsFalse(result.FlyBird, "The first run must only record a starting point.");
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.LastPollingTime);
    }

    [TestMethod]
    public async Task OnArticleCreated_LookingBackAYear_FindsArticles()
    {
        var result = await Events.OnArticleCreated(new PollingEventRequest<ArticleCreatedMemory>
        {
            Memory = new ArticleCreatedMemory { LastPollingTime = DateTime.UtcNow.AddYears(-1) }
        }, CreatedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, articles: {result.Result?.TotalCount ?? 0}");

        Assert.IsNotNull(result.Memory?.LastPollingTime);
        Assert.AreEqual(result.FlyBird, result.Result is not null);

        if (result.Result is not null)
        {
            Assert.IsTrue(result.Result.TotalCount > 0);
            Assert.IsTrue(result.Result.Articles.All(x => x.WorkspaceId == TestData.WorkspaceId));
        }
    }

    [TestMethod]
    public async Task OnArticleCreated_NothingCreatedSince_DoesNotFly()
    {
        var result = await Events.OnArticleCreated(new PollingEventRequest<ArticleCreatedMemory>
        {
            Memory = new ArticleCreatedMemory { LastPollingTime = DateTime.UtcNow }
        }, CreatedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}");

        Assert.IsFalse(result.FlyBird);
        Assert.IsNull(result.Result);
    }

    [TestMethod]
    public async Task OnArticlePublished_FirstRun_RecordsLiveVersions()
    {
        var result = await Events.OnArticlePublished(
            new PollingEventRequest<ArticlePublishedMemory>(), PublishedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, remembered: {result.Memory?.PublishedVersions?.Count}");

        Assert.IsFalse(result.FlyBird, "The first run must only record which versions are live.");
        Assert.IsNull(result.Result);
        Assert.IsTrue(result.Memory?.PublishedVersions?.Count > 0);
    }

    [TestMethod]
    public async Task OnArticlePublished_UnchangedSincePreviousRun_DoesNotFly()
    {
        var baseline = await Events.OnArticlePublished(
            new PollingEventRequest<ArticlePublishedMemory>(), PublishedFilter);

        var result = await Events.OnArticlePublished(new PollingEventRequest<ArticlePublishedMemory>
        {
            Memory = baseline.Memory
        }, PublishedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}");

        Assert.IsFalse(result.FlyBird, "Nothing was published between the two runs.");
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.PublishedVersions);
    }

    [TestMethod]
    public async Task OnArticlePublished_EmptyMemory_FindsEverythingLive()
    {
        var result = await Events.OnArticlePublished(new PollingEventRequest<ArticlePublishedMemory>
        {
            Memory = new ArticlePublishedMemory
            {
                LastPollingTime = DateTime.UtcNow.AddDays(-1),
                PublishedVersions = new Dictionary<string, int>()
            }
        }, PublishedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, articles: {result.Result?.TotalCount ?? 0}");

        Assert.IsTrue(result.FlyBird, "Every live article counts as newly published against an empty memory.");
        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result.Articles.All(x => x.PublishedVersion > 0));
    }
}
