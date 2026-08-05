using Apps.Document360.Actions;
using Tests.Document360.Base;

namespace Tests.Document360;

[TestClass]
public class CustomFieldActionTests : TestBase
{
    [TestMethod]
    public async Task SearchCustomFields_ReturnsDefinitionsWithOptions()
    {
        var actions = new CustomFieldActions(InvocationContext);

        var result = await actions.SearchCustomFields();

        foreach (var field in result.CustomFields)
            Console.WriteLine(
                $"{field.Name} ({field.Type}) options: {string.Join(", ", field.Options.Select(x => x.Label))}");

        Assert.IsNotNull(result.CustomFields);
        Assert.IsTrue(result.TotalCount > 0);
        Assert.IsTrue(result.CustomFields.All(x => !string.IsNullOrWhiteSpace(x.FieldId)));
        Assert.IsTrue(result.CustomFields.All(x => x.Options is not null),
            "Fields without choices must return an empty option list, never null.");
    }
}
