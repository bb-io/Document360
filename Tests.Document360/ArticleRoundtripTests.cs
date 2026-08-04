using Apps.Document360.Actions;
using Apps.Document360.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Document360.Base;

namespace Tests.Document360;

/// <summary>
/// The download/upload translation round trip. The decisive test is the golden invariant: downloading and uploading
/// again without editing anything must leave the article untouched.
/// </summary>
[TestClass]
public class ArticleRoundtripTests : TestBase
{
    private ArticleActions Actions => new(InvocationContext, FileManager);

    private static DownloadArticleRequest SourceArticle => new()
    {
        WorkspaceId = TestData.WorkspaceId,
        ContentId = TestData.MarkdownArticleId,
        Locale = TestData.SourceLocale
    };

    private static DownloadArticleRequest TargetArticle => new()
    {
        WorkspaceId = TestData.WorkspaceId,
        ContentId = TestData.MarkdownArticleId,
        Locale = TestData.TargetLocale
    };

    [TestMethod]
    public async Task DownloadArticle_ProducesSelfDescribingFile()
    {
        var result = await Actions.DownloadArticle(SourceArticle, new DownloadArticleOptions());
        var html = FileManager.ReadOutput(result.Content.Name);

        Console.WriteLine(html);

        Assert.IsTrue(result.Content.Name.EndsWith($"_{TestData.SourceLocale}.html"),
            "The file name has to end with the locale so source and target files pair up.");

        // The head metadata the upload side and the content lake read back.
        Assert.IsTrue(html.Contains($"lang=\"{TestData.SourceLocale}\""), "The locale belongs in <html lang>.");
        Assert.IsTrue(html.Contains("name=\"blackbird-ucid\""), "The article id belongs in blackbird-ucid.");
        Assert.IsTrue(html.Contains($"content=\"{TestData.MarkdownArticleId}\""));
        Assert.IsTrue(html.Contains("blackbird-content-name"));
        Assert.IsTrue(html.Contains("blackbird-system-name"));

        // The reconstruction attributes.
        Assert.IsTrue(html.Contains($"data-document360-article-id=\"{TestData.MarkdownArticleId}\""));
        Assert.IsTrue(html.Contains("data-document360-field-id=\"title\""));
        Assert.IsTrue(html.Contains("data-document360-field-id=\"content\""));
        Assert.IsTrue(html.Contains("data-document360-field-type=\"markdown\""));
        Assert.IsTrue(html.Contains($"data-blackbird-key=\"{TestData.MarkdownArticleId}-title\""));
        Assert.IsTrue(html.Contains("data-document360-source="), "The untranslated body has to travel with the file.");
        Assert.IsTrue(html.Contains("its-rev-tool=\"Document360\""));

        Assert.AreEqual("Markdown", result.EditorFormat);
        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
    }

    [TestMethod]
    public async Task DownloadArticle_SpecificVersion_ReturnsThatVersion()
    {
        var result = await Actions.DownloadArticle(TargetArticle, new DownloadArticleOptions { VersionNumber = 1 });

        Console.WriteLine($"Downloaded {result.Content.Name} from version {result.VersionNumber}");

        Assert.AreEqual(1, result.VersionNumber);
    }

    [TestMethod]
    public async Task DownloadArticle_UnknownLocale_Fails()
    {
        var request = new DownloadArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            ContentId = TestData.MarkdownArticleId,
            Locale = "zz"
        };

        var exception = await Assert.ThrowsExceptionAsync<Blackbird.Applications.Sdk.Common.Exceptions.
            PluginMisconfigurationException>(() => Actions.DownloadArticle(request, new DownloadArticleOptions()));

        Console.WriteLine(exception.Message);
        Assert.IsTrue(exception.Message.Contains("not enabled for this workspace"));
    }

    /// <summary>The golden invariant: a round trip with no edits must not touch the article.</summary>
    [TestMethod]
    public async Task UploadArticle_WithoutEdits_ChangesNothing()
    {
        var downloaded = await Actions.DownloadArticle(TargetArticle, new DownloadArticleOptions());
        var file = FileManager.CopyOutputToInput(downloaded.Content.Name);

        var result = await Actions.UploadArticle(new UploadArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            Locale = TestData.TargetLocale,
            Content = file,
            UserId = TestData.UserId
        });

        Console.WriteLine($"Updated fields: {(result.UpdatedFields.Count == 0 ? "(none)" : string.Join(", ", result.UpdatedFields))}");

        Assert.AreEqual(0, result.UpdatedFields.Count,
            "Uploading an unedited file must not write anything back to Document360.");
        Assert.IsFalse(result.WasCopiedToNewDraft, "Nothing changed, so no new version should have been created.");
        Assert.AreEqual(TestData.MarkdownArticleId, result.ArticleId);
        Assert.IsNotNull(result.Content);
    }

    [TestMethod]
    public async Task UploadArticle_TranslatedTitleAndBody_WritesOnlyThose()
    {
        var downloaded = await Actions.DownloadArticle(TargetArticle, new DownloadArticleOptions());
        var html = FileManager.ReadOutput(downloaded.Content.Name);

        var stamp = DateTime.UtcNow.ToString("HHmmss");
        var translatedTitle = $"Titre traduit {stamp}";

        // Replace the title text and the first body line, the way a translation app would.
        var edited = ReplaceFirstElementText(html, "h1", translatedTitle);
        edited = ReplaceFirstElementText(edited, "p", $"## Apercu traduit {stamp}");

        var file = FileManager.WriteInput($"translated_{stamp}.html", edited);

        var result = await Actions.UploadArticle(new UploadArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            Locale = TestData.TargetLocale,
            Content = file,
            UserId = TestData.UserId
        });

        Console.WriteLine($"Updated fields: {string.Join(", ", result.UpdatedFields)}");
        Console.WriteLine($"Copied to new draft: {result.WasCopiedToNewDraft}, version {result.VersionNumber}");

        Assert.IsTrue(result.UpdatedFields.Contains("Title"));
        Assert.IsTrue(result.UpdatedFields.Contains("Content"));
        Assert.AreEqual("Draft", result.Status);

        // Confirm against the API that only the translated values landed, with the markdown structure intact.
        var stored = await Actions.GetArticleInfo(new GetArticleInfoRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            ArticleId = TestData.MarkdownArticleId,
            Locale = TestData.TargetLocale
        });

        Console.WriteLine(stored.Content);

        Assert.AreEqual(translatedTitle, stored.Title);
        Assert.IsTrue(stored.Content.Contains($"## Apercu traduit {stamp}"));
        Assert.IsTrue(stored.Content.Contains("\n\n"), "Blank lines of the markdown body have to survive.");
    }

    [TestMethod]
    public async Task UploadArticle_UnsupportedFileType_Fails()
    {
        var file = FileManager.WriteInput("not-translatable.txt", "plain text");

        var exception = await Assert.ThrowsExceptionAsync<Blackbird.Applications.Sdk.Common.Exceptions.
            PluginMisconfigurationException>(() => Actions.UploadArticle(new UploadArticleRequest
        {
            WorkspaceId = TestData.WorkspaceId,
            Locale = TestData.TargetLocale,
            Content = file
        }));

        Console.WriteLine(exception.Message);
        Assert.IsTrue(exception.Message.Contains(".html"));
    }

    private static string ReplaceFirstElementText(string html, string tag, string replacement)
    {
        var open = html.IndexOf($"<{tag} ", StringComparison.Ordinal);
        Assert.IsTrue(open >= 0, $"The file should contain a <{tag}> element.");

        var textStart = html.IndexOf('>', open) + 1;
        var textEnd = html.IndexOf($"</{tag}>", textStart, StringComparison.Ordinal);
        Assert.IsTrue(textEnd > textStart, $"The <{tag}> element should hold text.");

        return html[..textStart] + replacement + html[textEnd..];
    }
}
