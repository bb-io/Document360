using Apps.Document360.Events;
using Apps.Document360.Models.Polling;
using Apps.Document360.Models.Responses;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Tests.Document360.Base;

namespace Tests.Document360;

[TestClass]
public class ArticlePollingTests : TestBase
{
    private ArticlePollingList Events => new(InvocationContext);

    private static ArticlesCreatedFilter CreatedFilter => new()
    {
        WorkspaceId = TestData.WorkspaceId,
        Locale = TestData.SourceLocale
    };

    private static ArticlesPublishedFilter PublishedFilter => new()
    {
        WorkspaceId = TestData.WorkspaceId,
        Locale = TestData.SourceLocale
    };

    [TestMethod]
    public async Task OnArticlesCreated_FirstRun_DoesNotFly()
    {
        var result = await Events.OnArticlesCreated(new PollingEventRequest<PollingMemory>(), CreatedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, memory: {result.Memory?.LastPollingTime}");

        Assert.IsFalse(result.FlyBird, "The first run must only record a starting point.");
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.LastPollingTime);
    }

    [TestMethod]
    public async Task OnArticlesCreated_LookingBackAYear_FindsArticles()
    {
        var result = await Events.OnArticlesCreated(new PollingEventRequest<PollingMemory>
        {
            Memory = new PollingMemory { LastPollingTime = DateTime.UtcNow.AddYears(-1) }
        }, CreatedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, articles: {result.Result?.TotalCount ?? 0}");

        Assert.IsNotNull(result.Memory?.LastPollingTime);
        Assert.AreEqual(result.FlyBird, result.Result is not null);

        if (result.Result is not null)
        {
            Assert.IsTrue(result.Result.TotalCount > 0);
            Assert.AreEqual(result.Result.Items.Count, result.Result.TotalCount);
            Assert.IsTrue(result.Result.Items.All(x => x.WorkspaceId == TestData.WorkspaceId));
        }
    }

    [TestMethod]
    public async Task OnArticlesCreated_NothingCreatedSince_DoesNotFly()
    {
        var result = await Events.OnArticlesCreated(new PollingEventRequest<PollingMemory>
        {
            Memory = new PollingMemory { LastPollingTime = DateTime.UtcNow }
        }, CreatedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}");

        Assert.IsFalse(result.FlyBird);
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.LastPollingTime);
    }

    [TestMethod]
    public async Task OnArticlesPublished_FirstRun_RecordsLiveVersions()
    {
        var result = await Events.OnArticlesPublished(
            new PollingEventRequest<PublishedVersionsMemory>(), PublishedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, remembered: {result.Memory?.PublishedVersions?.Count}");

        Assert.IsFalse(result.FlyBird, "The first run must only record which versions are live.");
        Assert.IsNull(result.Result);
        Assert.IsTrue(result.Memory?.PublishedVersions?.Count > 0);
    }

    [TestMethod]
    public async Task OnArticlesPublished_UnchangedSincePreviousRun_DoesNotFly()
    {
        var baseline = await Events.OnArticlesPublished(
            new PollingEventRequest<PublishedVersionsMemory>(), PublishedFilter);

        var result = await Events.OnArticlesPublished(new PollingEventRequest<PublishedVersionsMemory>
        {
            Memory = baseline.Memory
        }, PublishedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}");

        Assert.IsFalse(result.FlyBird, "Nothing was published between the two runs.");
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Memory?.PublishedVersions);
    }

    [TestMethod]
    public async Task OnArticlesPublished_EmptyMemory_FindsEverythingLive()
    {
        var result = await Events.OnArticlesPublished(new PollingEventRequest<PublishedVersionsMemory>
        {
            Memory = new PublishedVersionsMemory
            {
                LastPollingTime = DateTime.UtcNow.AddDays(-1),
                PublishedVersions = new Dictionary<string, int>()
            }
        }, PublishedFilter);

        Console.WriteLine($"Fly bird: {result.FlyBird}, articles: {result.Result?.TotalCount ?? 0}");

        Assert.IsTrue(result.FlyBird, "Every live article counts as newly published against an empty memory.");
        Assert.IsNotNull(result.Result);
    }


    [TestMethod]
    public async Task OnArticlesPublished_FulfilsTheContentCreatedOrUpdatedBlueprint()
    {
        var definition = typeof(ArticlePollingList)
            .GetMethod(nameof(ArticlePollingList.OnArticlesPublished))!
            .GetCustomAttributes(typeof(BlueprintEventDefinitionAttribute), false)
            .Cast<BlueprintEventDefinitionAttribute>()
            .SingleOrDefault();

        Assert.IsNotNull(definition, "The event has to declare the blueprint it fulfils.");
        Assert.AreEqual(BlueprintEvent.ContentCreatedOrUpdatedMultiple, definition.BlueprintItem);

        Assert.IsInstanceOfType<IMultiDownloadableContentOutput<ArticleListItemResponse>>(
            new ArticlesEventResponse(), "The output has to be a blueprint content collection.");
        Assert.IsInstanceOfType<IDownloadContentInput>(new ArticleListItemResponse(),
            "Every item has to be usable as the input of a download action.");

        var result = await Events.OnArticlesPublished(new PollingEventRequest<PublishedVersionsMemory>
        {
            Memory = new PublishedVersionsMemory
            {
                LastPollingTime = DateTime.UtcNow.AddDays(-1),
                PublishedVersions = new Dictionary<string, int>()
            }
        }, PublishedFilter);

        Assert.IsNotNull(result.Result);
        Assert.IsTrue(result.Result.Items.All(x => !string.IsNullOrWhiteSpace(x.ContentId)),
            "A blueprint item without a content id cannot be downloaded.");
    }
}
