using Apps.Document360.Handlers;
using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Identifiers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Tests.Document360.Base;

namespace Tests.Document360;

[TestClass]
public class DataSourceHandlerTests : TestBase
{
    private static DataSourceContext EmptyContext => new() { SearchString = null };

    private static WorkspaceIdentifier Workspace => new() { WorkspaceId = TestData.WorkspaceId };

    [TestMethod]
    public async Task WorkspaceHandler_ReturnsWorkspaces()
    {
        var items = (await new WorkspaceDataHandler(InvocationContext)
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        foreach (var item in items) Console.WriteLine($"{item.Value} - {item.DisplayName}");

        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task LocaleHandler_WithWorkspace_ReturnsLocales()
    {
        var items = (await new LocaleDataHandler(InvocationContext, Workspace)
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        foreach (var item in items) Console.WriteLine($"{item.Value} - {item.DisplayName}");

        Assert.IsTrue(items.Count > 0);
        Assert.IsTrue(items.Any(x => x.Value == TestData.TargetLocale));
    }

    [TestMethod]
    public async Task LocaleHandler_WithoutWorkspace_StillReturnsLocales()
    {
        var items = (await new LocaleDataHandler(InvocationContext, new WorkspaceIdentifier())
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        Console.WriteLine($"Locales across the project: {items.Count}");

        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task CategoryHandler_ReturnsFlattenedPaths()
    {
        var items = (await new CategoryDataHandler(InvocationContext, Workspace)
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        foreach (var item in items.Take(10)) Console.WriteLine(item.DisplayName);

        Assert.IsTrue(items.Count > 0);
        Assert.IsTrue(items.Any(x => x.DisplayName.Contains(" / ")), "Nested categories should show their path.");
    }

    [TestMethod]
    public async Task ArticleHandler_ReturnsArticles()
    {
        var items = (await new ArticleDataHandler(InvocationContext, Workspace)
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        Console.WriteLine($"Articles offered: {items.Count}");

        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task ArticleHandler_WithoutWorkspace_ReturnsEmpty()
    {
        var items = (await new ArticleDataHandler(InvocationContext, new WorkspaceIdentifier())
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        Assert.AreEqual(0, items.Count);
    }

    [TestMethod]
    public async Task TeamAccountHandler_ReturnsAccounts()
    {
        var items = (await new TeamAccountDataHandler(InvocationContext)
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        foreach (var item in items.Take(5)) Console.WriteLine($"{item.Value} - {item.DisplayName}");

        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task ArticleVersionHandler_ReturnsVersions()
    {
        var article = new ArticleIdentifier
        {
            WorkspaceId = TestData.WorkspaceId,
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.TargetLocale
        };

        var items = (await new ArticleVersionDataHandler(InvocationContext, article)
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        foreach (var item in items) Console.WriteLine($"{item.Value} - {item.DisplayName}");

        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public async Task CustomFieldHandler_ReturnsDefinitions()
    {
        var items = (await new CustomFieldDataHandler(InvocationContext)
            .GetDataAsync(EmptyContext, CancellationToken.None)).ToList();

        foreach (var item in items) Console.WriteLine($"{item.Value} - {item.DisplayName}");

        Assert.IsTrue(items.Count > 0);
    }

    [TestMethod]
    public void StaticHandlers_ReturnUniqueValues()
    {
        AssertUniqueValues(new EditorFormatDataHandler().GetData());
        AssertUniqueValues(new TranslationStatusDataHandler().GetData());
        AssertUniqueValues(new VisibilityDataHandler().GetData());
        AssertUniqueValues(new PublicationStatusDataHandler().GetData());
    }

    private static void AssertUniqueValues(IEnumerable<DataSourceItem> items)
    {
        var list = items.ToList();

        Assert.IsTrue(list.Count > 0);
        Assert.AreEqual(list.Count, list.Select(x => x.Value).Distinct().Count(),
            "Static data source values have to be unique.");
    }
}
