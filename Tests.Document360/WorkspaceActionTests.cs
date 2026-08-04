using Apps.Document360.Actions;
using Apps.Document360.Models.Identifiers;
using Tests.Document360.Base;

namespace Tests.Document360;

[TestClass]
public class WorkspaceActionTests : TestBase
{
    private WorkspaceActions Actions => new(InvocationContext);

    [TestMethod]
    public async Task SearchWorkspaces_ReturnsWorkspacesWithLocales()
    {
        var result = await Actions.SearchWorkspaces();

        foreach (var workspace in result.Workspaces)
            Console.WriteLine(
                $"{workspace.Name} ({workspace.WorkspaceId}): {string.Join(", ", workspace.Locales.Select(x => x.Locale))}");

        Assert.IsNotNull(result.Workspaces);
        Assert.IsTrue(result.TotalCount > 0);
        Assert.IsTrue(result.Workspaces.All(x => !string.IsNullOrWhiteSpace(x.WorkspaceId)));
        Assert.IsTrue(result.Workspaces.All(x => !string.IsNullOrWhiteSpace(x.Name)));
    }

    [TestMethod]
    public async Task SearchWorkspaceLocales_ReturnsExactlyOneDefault()
    {
        var result = await Actions.SearchWorkspaceLocales(new WorkspaceIdentifier
        {
            WorkspaceId = TestData.WorkspaceId
        });

        foreach (var locale in result.Locales)
            Console.WriteLine($"{locale.Locale} - {locale.Name} (default: {locale.IsDefault})");

        Assert.IsTrue(result.TotalCount > 0);
        Assert.AreEqual(1, result.Locales.Count(x => x.IsDefault));
    }
}
