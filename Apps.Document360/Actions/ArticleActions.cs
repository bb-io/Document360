using System.Net.Mime;
using System.Text;
using Apps.Document360.Constants;
using Apps.Document360.Extensions;
using Apps.Document360.Handlers.Static;
using Apps.Document360.Models.Dtos;
using Apps.Document360.Models.Entities;
using Apps.Document360.Models.Identifiers;
using Apps.Document360.Models.Requests;
using Apps.Document360.Models.Responses;
using Apps.Document360.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Constants;
using Blackbird.Filters.Enums;
using Blackbird.Filters.Extensions;
using Blackbird.Filters.Shared;
using Blackbird.Filters.Transformations;

namespace Apps.Document360.Actions;

// TODO: don't use services. All code for API calls should be inside action classes and actions should orchestrate them. If you see code duplication add Provider class or Helper instead but don't add Services!

[ActionList("Articles")]
public class ArticleActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : Invocable(invocationContext)
{
    private const int DescriptionMaxLength = 250;

    private static readonly string[] SupportedUploadExtensions = [".html", ".xliff", ".xlf"];

    [Action("Create draft article",
        Description = "Create a new draft article in a category, with its body taken from a file.")]
    public async Task<CreateArticleResponse> CreateDraftArticle([ActionParameter] CreateDraftArticleRequest request)
    {
        RequireWorkspace(request);
        Require(request.Title, "Title");
        Require(request.CategoryId, "Category ID");
        Require(request.UserId, "User ID");

        if (request.File is null)
            throw new PluginMisconfigurationException(
                "The article body is missing. Please fill in 'File' with the file holding the article text.");

        var body = new Dictionary<string, object>
        {
            ["title"] = request.Title,
            ["content"] = await ReadFileAsTextAsync(request.File),
            ["category_id"] = request.CategoryId,
            ["project_version_id"] = request.WorkspaceId,
            ["user_id"] = request.UserId
        };

        var editorFormat = ResolveEditorFormat(request.EditorFormat, request.File.Name);
        body.AddIfNotNull("content_type", editorFormat)
            .AddIfNotEmpty("slug", request.Slug)
            .AddIfNotNull("order", request.Order);

        var created = await Articles.CreateArticleAsync(body);

        // The create response carries no URL, category or workspace, so the new draft is read back once.
        var locale = string.IsNullOrWhiteSpace(created.LanguageCode)
            ? await ResolveDefaultLocaleAsync(request.WorkspaceId)
            : created.LanguageCode;

        var article = await Articles.GetArticleAsync(created.Id, locale);
        return new CreateArticleResponse(article, locale);
    }

    [Action("Update article",
        Description =
            "Update the content and metadata of an article translation, leaving it as an editable draft. " +
            "A locale that has never been translated is created on first use.")]
    public async Task<UpdateArticleResponse> UpdateArticle([ActionParameter] UpdateArticleRequest request)
    {
        RequireArticle(request);

        if (request.File is not null && !string.IsNullOrEmpty(request.Content))
            throw new PluginMisconfigurationException(
                "The article body was supplied twice. Please fill in either 'File' or 'Content', not both.");

        RequireDescriptionFits(request.Description);

        var customFields = BuildCustomFields(request.CustomFieldIds, request.CustomFieldValues);
        var current = await Articles.GetArticleAsync(request.ArticleId, request.Locale);

        var contentBody = new Dictionary<string, object>()
            .AddIfNotEmpty("title", request.Title)
            .AddIfNotNull("content",
                request.File is not null ? await ReadFileAsTextAsync(request.File) : request.Content)
            // Document360 rejects this field unless it is sent as text, even though it holds a number.
            .AddIfNotEmpty("translation_option", request.TranslationStatus)
            .AddIfNotNull("hidden", request.Hidden)
            .AddIfNotEmpty("category_id", request.CategoryId)
            .AddIfNotNull("order", request.Order);

        var wasCopiedToNewDraft =
            await WriteArticleContentAsync(request.ArticleId, request.Locale, contentBody, current, request.UserId);

        if (request.Description is not null)
            await Articles.UpdateArticleDescriptionAsync(request.ArticleId, request.Locale, request.Description);

        var settingsBody = new Dictionary<string, object>()
            .AddIfNotEmpty("seo_title", request.SeoTitle)
            .AddIfNotEmpty("description", request.MetaDescription);

        if (request.Tags is not null) settingsBody["tags"] = request.Tags.ToList();
        if (customFields.Count > 0) settingsBody["custom_fields"] = customFields;

        if (settingsBody.Count > 0)
            await Articles.UpdateArticleSettingsAsync(request.ArticleId, request.Locale, settingsBody);

        // The write responses report stale values, so stored state is read back for the outputs.
        var article = await Articles.GetArticleAsync(request.ArticleId, request.Locale);
        var settings = await TryGetSettingsAsync(request.ArticleId, request.Locale);

        return new UpdateArticleResponse(article, settings, request.Locale, wasCopiedToNewDraft);
    }

    [Action("Publish article", Description = "Put an article translation live on the knowledge base site.")]
    public async Task<PublishArticleResponse> PublishArticle([ActionParameter] PublishArticleRequest request)
    {
        RequireArticle(request);
        Require(request.UserId, "User ID");

        if (request.VersionNumber is <= 0)
            throw new PluginMisconfigurationException(
                "'Version number' has to be 1 or higher. Leave it empty to publish the newest version.");

        var version = request.VersionNumber ?? await ResolveLatestVersionAsync(request.ArticleId, request.Locale);

        var body = new Dictionary<string, object>
        {
            ["user_id"] = request.UserId,
            ["version_number"] = version
        };
        body.AddIfNotEmpty("publish_message", request.PublishMessage);

        var publishResult = await Articles.PublishArticleAsync(request.ArticleId, request.Locale, body);
        var published = await TryGetArticleAsync(request.ArticleId, request.Locale, isPublished: true);

        return new PublishArticleResponse
        {
            ArticleUrl = publishResult.Url ?? published?.Url ?? string.Empty,
            PublishedVersion = published?.PublicVersion ?? version,
            Status = published is null ? "Published" : Document360Enums.ArticleStatus(published.Status),
            WasAlreadyLive = publishResult.WasAlreadyLive
        };
    }

    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    [Action("Download article",
        Description =
            "Get the translatable fields of an article translation as one file that describes itself, ready to be " +
            "translated and sent back with 'Upload article'.")]
    public async Task<ArticleContentResponse> DownloadArticle(
        [ActionParameter] DownloadArticleRequest request,
        [ActionParameter] DownloadArticleOptions options)
    {
        RequireWorkspace(request);
        Require(request.ContentId, "Article ID");
        Require(request.Locale, "Locale");

        if (options.VersionNumber.HasValue && options.UsePublishedVersion == true)
            throw new PluginMisconfigurationException(
                "'Version number' and 'Use published version' cannot be combined. Choose one version to download.");

        if (options.VersionNumber is <= 0)
            throw new PluginMisconfigurationException("'Version number' has to be 1 or higher.");

        await RequireLocaleExistsAsync(request.WorkspaceId, request.Locale);

        var article = options.VersionNumber.HasValue
            ? await Articles.GetArticleVersionAsync(request.ContentId, request.Locale, options.VersionNumber.Value,
                options.ResolveMergeCodes ?? false)
            : await Articles.GetArticleAsync(request.ContentId, request.Locale, options.UsePublishedVersion ?? false,
                options.ResolveMergeCodes ?? false);

        var settings = await TryGetSettingsAsync(request.ContentId, request.Locale);
        var fields = BuildTranslatableFields(article, settings);

        var fileName = string.IsNullOrWhiteSpace(options.FileName)
            ? ArticleHtmlBuilder.BuildFileName(article.Title, article.Id, request.Locale)
            : options.FileName;

        var html = ArticleHtmlBuilder.Build(fields, request.Locale, fileName, article.Url,
            ResolveSystemReference(article.Url), DescribeAuthor(article));

        await using var stream = new MemoryStream(new UTF8Encoding(false).GetBytes(html));

        return new ArticleContentResponse
        {
            Content = await fileManagementClient.UploadAsync(stream, MediaTypeNames.Text.Html, fileName),
            ArticleId = article.Id,
            Locale = request.Locale,
            Title = article.Title ?? string.Empty,
            ArticleUrl = article.Url ?? string.Empty,
            EditorFormat = Document360Enums.EditorFormat(article.ContentType),
            VersionNumber = article.VersionNumber ?? 0,
            Status = Document360Enums.ArticleStatus(article.Status),
            TranslationStatus = Document360Enums.TranslationStatus(article.TranslationOption),
            IsFallbackContent = article.IsFallBackContent ?? false
        };
    }

    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    [Action("Upload article",
        Description =
            "Write a translated file back onto an article translation, leaving it as an editable draft. Only the " +
            "fields that actually changed are written.")]
    public async Task<ArticleContentResponse> UploadArticle([ActionParameter] UploadArticleRequest request)
    {
        RequireWorkspace(request);
        Require(request.Locale, "Locale");

        if (request.Content is null)
            throw new PluginMisconfigurationException(
                "The translated file is missing. Please fill in 'File' with the file to write back.");

        if (!SupportedUploadExtensions.Any(extension =>
                (request.Content.Name ?? string.Empty).EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
            throw new PluginMisconfigurationException(
                $"'{request.Content.Name}' cannot be read. Please supply an .html, .xliff or .xlf file.");

        await RequireLocaleExistsAsync(request.WorkspaceId, request.Locale);

        var stream = await fileManagementClient.DownloadAsync(request.Content);
        var loadResult = Transformation.Load(stream, request.Content.Name, request.Content.ContentType);

        // Target() hands back the translated side whether the file was bilingual XLIFF or plain HTML. A file that is
        // not Blackbird-interoperable at all is read as raw HTML instead.
        var target = loadResult.Target();
        var html = target.Success
            ? target.Value.ToStream(MetadataHandling.Include).ReadString()
            : (await fileManagementClient.DownloadAsync(request.Content)).ReadString();

        var fields = ArticleHtmlReader.Read(html);
        var articleId = string.IsNullOrWhiteSpace(request.ContentId) ? fields.ArticleId : request.ContentId;

        if (string.IsNullOrWhiteSpace(articleId))
            throw new PluginMisconfigurationException(
                "The file does not say which article it belongs to. Please fill in 'Article ID', or use a file " +
                "produced by 'Download article'.");

        RequireDescriptionFits(fields.Description);

        var current = await Articles.GetArticleAsync(articleId, request.Locale);
        var currentSettings = await TryGetSettingsAsync(articleId, request.Locale);
        var updatedFields = new List<string>();
        var contentBody = new Dictionary<string, object>();

        if (HasChanged(fields.Title, current.Title))
        {
            contentBody["title"] = fields.Title!;
            updatedFields.Add("Title");
        }

        if (fields.Body is not null && !ArticleHtmlReader.IsSameContent(fields.Body, ArticleContent.Select(current)))
        {
            contentBody["content"] = fields.Body;
            updatedFields.Add("Content");
        }

        var wasCopiedToNewDraft =
            await WriteArticleContentAsync(articleId, request.Locale, contentBody, current, request.UserId);

        if (HasChanged(fields.Description, current.Description))
        {
            await Articles.UpdateArticleDescriptionAsync(articleId, request.Locale, fields.Description!);
            updatedFields.Add("Description");
        }

        var settingsBody = new Dictionary<string, object>();

        if (HasChanged(fields.SeoTitle, currentSettings?.SeoTitle))
        {
            settingsBody["seo_title"] = fields.SeoTitle!;
            updatedFields.Add("SEO title");
        }

        if (HasChanged(fields.MetaDescription, currentSettings?.Description))
        {
            settingsBody["description"] = fields.MetaDescription!;
            updatedFields.Add("Meta description");
        }

        if (settingsBody.Count > 0)
            await Articles.UpdateArticleSettingsAsync(articleId, request.Locale, settingsBody);

        var article = await Articles.GetArticleAsync(articleId, request.Locale);

        return new ArticleContentResponse
        {
            Content = await BuildUploadedFileAsync(request, loadResult, article),
            ArticleId = article.Id,
            Locale = request.Locale,
            Title = article.Title ?? string.Empty,
            ArticleUrl = article.Url ?? string.Empty,
            EditorFormat = Document360Enums.EditorFormat(article.ContentType),
            VersionNumber = article.VersionNumber ?? 0,
            Status = Document360Enums.ArticleStatus(article.Status),
            TranslationStatus = Document360Enums.TranslationStatus(article.TranslationOption),
            WasCopiedToNewDraft = wasCopiedToNewDraft,
            UpdatedFields = updatedFields
        };
    }

    [Action("Get article info",
        Description = "Get all details of an article translation, including its tags, custom fields and languages.")]
    public async Task<ArticleInfoResponse> GetArticleInfo([ActionParameter] GetArticleInfoRequest request)
    {
        RequireArticle(request);

        var article = await Articles.GetArticleAsync(request.ArticleId, request.Locale,
            request.UsePublishedVersion ?? false);
        var settings = await TryGetSettingsAsync(request.ArticleId, request.Locale);

        return new ArticleInfoResponse(article, settings, request.Locale);
    }

    [Action("Search articles", Description = "Find the articles of a workspace that match your filters.")]
    public async Task<SearchArticlesResponse> SearchArticles([ActionParameter] SearchArticlesRequest request)
    {
        RequireWorkspace(request);

        if (!string.IsNullOrWhiteSpace(request.TranslationStatus) && string.IsNullOrWhiteSpace(request.Locale))
            throw new PluginMisconfigurationException(
                "Filtering by translation status needs to know which language to look at. Please fill in 'Locale'.");

        if (request.MaxResults is <= 0)
            throw new PluginMisconfigurationException(
                "'Max results' has to be 1 or higher. Leave it empty to return every match.");

        var visibility = ParseOption(request.Visibility, "Visibility");
        var articles = await Articles.GetArticlesAsync(request.WorkspaceId, request.Locale, visibility);

        if (!string.IsNullOrWhiteSpace(request.TranslationStatus))
        {
            var translationStatus = ParseOption(request.TranslationStatus, "Translation status")!.Value;
            var matching = await Articles.GetTranslationArticleIdsAsync(request.WorkspaceId, request.Locale!,
                translationStatus);
            articles = articles.Where(article => matching.Contains(article.Id)).ToList();
        }

        // An article is live when it has a published version. Its status only describes the newest version, so an
        // article that is live and has a newer draft still reports "Draft".
        articles = request.PublicationStatus switch
        {
            PublicationStatusDataHandler.Published => articles.Where(x => (x.PublicVersion ?? 0) > 0).ToList(),
            PublicationStatusDataHandler.Draft => articles.Where(x => (x.PublicVersion ?? 0) == 0).ToList(),
            _ => articles
        };

        if (request.Hidden.HasValue)
            articles = articles.Where(x => (x.Hidden ?? false) == request.Hidden.Value).ToList();

        if (request.ModifiedAfter.HasValue)
        {
            var threshold = request.ModifiedAfter.Value.ToUniversalTime();
            articles = articles
                .Where(x => x.ModifiedAt.HasValue && x.ModifiedAt.Value.ToUniversalTime() > threshold)
                .ToList();
        }

        if (request.MaxResults.HasValue) articles = articles.Take(request.MaxResults.Value).ToList();

        var results = articles
            .Select(article => new ArticleListItemResponse(article, request.WorkspaceId))
            .ToList();

        return new SearchArticlesResponse { Articles = results, TotalCount = results.Count };
    }

    #region Writing content

    /// <summary>
    /// Writes the content fields, if there are any. Content can only be written to a draft, so a published,
    /// unpublished or in-review version is copied into a new draft first — Document360 calls that forking.
    /// </summary>
    /// <returns>Whether the version had to be copied into a new draft.</returns>
    private async Task<bool> WriteArticleContentAsync(string articleId, string locale,
        Dictionary<string, object> contentBody, ArticleDto current, string? userId)
    {
        if (contentBody.Count == 0) return false;

        var wasCopiedToNewDraft = false;

        if (current.Status != Document360Enums.ArticleStatusDraft)
        {
            await CopyToNewDraftAsync(articleId, locale, current, userId);
            wasCopiedToNewDraft = true;
        }

        if (await Articles.TryUpdateArticleAsync(articleId, locale, contentBody)) return wasCopiedToNewDraft;

        if (wasCopiedToNewDraft) throw ReadOnlyAfterCopy();

        await CopyToNewDraftAsync(articleId, locale, current, userId);

        if (!await Articles.TryUpdateArticleAsync(articleId, locale, contentBody)) throw ReadOnlyAfterCopy();

        return true;
    }

    private async Task CopyToNewDraftAsync(string articleId, string locale, ArticleDto current, string? userId)
    {
        var author = string.IsNullOrWhiteSpace(userId) ? current.CreatedBy : userId;

        if (string.IsNullOrWhiteSpace(author))
            throw new PluginMisconfigurationException(
                "This article's newest version is read-only and has to be copied into a new draft before it can be " +
                "updated, which requires a team account. Please fill in 'User ID'.");

        var body = new Dictionary<string, object>
        {
            ["version_number"] = current.LatestVersion ?? current.VersionNumber ?? 1,
            ["user_id"] = author,
            ["lang_code"] = locale
        };

        await Articles.ForkArticleAsync(articleId, body);
    }

    private static PluginApplicationException ReadOnlyAfterCopy() =>
        new("Document360 kept the article in read-only mode even after it was copied into a new draft. " +
            "Check the article's workflow status in Document360 and try again.");

    #endregion

    #region Translation round trip

    private static ArticleTranslatableFields BuildTranslatableFields(ArticleDto article,
        ArticleSettingsDto? settings) =>
        new()
        {
            ArticleId = article.Id,
            Title = article.Title,
            Body = ArticleContent.Select(article),
            EditorFormat = article.ContentType,
            Description = article.Description,
            SeoTitle = settings?.SeoTitle,
            MetaDescription = settings?.Description
        };

    /// <summary>
    /// Returns the file that was written back, stamped with where the content now lives. A following "add to lake"
    /// step reads those references, which only exist once the article has actually been updated.
    /// </summary>
    private async Task<FileReference> BuildUploadedFileAsync(UploadArticleRequest request,
        TransformationLoadResult loadResult, ArticleDto article)
    {
        if (!loadResult.Success) return request.Content;

        var transformation = loadResult.Value;

        transformation.TargetSystemReference = new SystemReference
        {
            ContentId = article.Id,
            ContentName = article.Title ?? article.Id,
            PublicUrl = article.Url,
            SystemName = ArticleContentAttributes.SystemName,
            SystemRef = ResolveSystemReference(article.Url)
        };
        transformation.TargetLanguage = request.Locale;

        if (loadResult.WasBilingual)
            return await fileManagementClient.UploadAsync(transformation.ToStream(), MediaTypes.Xliff2,
                transformation.BilingualFileName);

        var target = transformation.Target();
        if (!target.Success) return request.Content;

        var content = target.Value;
        content.SystemReference = transformation.TargetSystemReference;

        return await fileManagementClient.UploadAsync(content.ToStream(MetadataHandling.Include),
            content.OriginalMediaType, content.OriginalName);
    }

    /// <summary>The knowledge base site the article belongs to, falling back to the regional API hub.</summary>
    private string ResolveSystemReference(string? articleUrl) =>
        Uri.TryCreate(articleUrl, UriKind.Absolute, out var uri)
            ? $"{uri.Scheme}://{uri.Host}"
            : Client.ApiHubUrl.ToString().TrimEnd('/');

    private static string? DescribeAuthor(ArticleDto article)
    {
        var author = article.Authors?.FirstOrDefault();
        if (author is null) return null;

        var name = $"{author.FirstName} {author.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? author.EmailId : name;
    }

    private static bool HasChanged(string? incoming, string? current) =>
        !string.IsNullOrWhiteSpace(incoming) && incoming != (current ?? string.Empty);

    #endregion

    #region Helpers

    private async Task<int> ResolveLatestVersionAsync(string articleId, string locale)
    {
        var versions = await Articles.GetArticleVersionsAsync(articleId, locale);

        if (versions.Count == 0)
            throw new PluginMisconfigurationException(
                "This article has no version in the chosen locale, so there is nothing to publish. Update the " +
                "article in that locale first.");

        return versions.Max(version => version.VersionNumber);
    }

    private async Task<string> ResolveDefaultLocaleAsync(string workspaceId)
    {
        var locales = await Workspaces.GetWorkspaceLocalesAsync(workspaceId);
        var defaultLocale =
            locales.FirstOrDefault(locale => locale.IsSetAsDefault == true) ?? locales.FirstOrDefault();

        return defaultLocale?.LanguageCode ??
               throw new PluginApplicationException(
                   "The article was created, but Document360 reported no language for the workspace, so it could " +
                   "not be read back. Use 'Get article info' to fetch its details.");
    }

    private async Task RequireLocaleExistsAsync(string workspaceId, string locale)
    {
        var locales = await Workspaces.GetWorkspaceLocalesAsync(workspaceId);
        if (locales.Any(x => string.Equals(x.LanguageCode, locale, StringComparison.OrdinalIgnoreCase))) return;

        throw new PluginMisconfigurationException(
            $"The locale '{locale}' is not enabled for this workspace. Available locales: " +
            $"{string.Join(", ", locales.Select(x => x.LanguageCode))}.");
    }

    /// <summary>Tags live only on the settings endpoint; losing them must not fail the whole action.</summary>
    private async Task<ArticleSettingsDto?> TryGetSettingsAsync(string articleId, string locale)
    {
        try
        {
            return await Articles.GetArticleSettingsAsync(articleId, locale);
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"[Document360] Could not read article settings: {ex.Message}", []);
            return null;
        }
    }

    private async Task<ArticleDto?> TryGetArticleAsync(string articleId, string locale, bool isPublished)
    {
        try
        {
            return await Articles.GetArticleAsync(articleId, locale, isPublished);
        }
        catch (Exception ex)
        {
            InvocationContext.Logger?.LogError($"[Document360] Could not confirm the published article: {ex.Message}",
                []);
            return null;
        }
    }

    private async Task<string> ReadFileAsTextAsync(FileReference file)
    {
        var stream = await fileManagementClient.DownloadAsync(file);
        var bytes = await stream.GetByteData();

        if (bytes.Length == 0)
            throw new PluginMisconfigurationException(
                $"The file '{file.Name}' is empty. Please supply a file that holds the article text.");

        return ArticleContent.DecodeUtf8(bytes);
    }

    private static int? ResolveEditorFormat(string? editorFormat, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(editorFormat)) return ParseOption(editorFormat, "Editor format");

        // Markdown files are stored as markdown so that a later download returns markdown again. Anything else is
        // left to the project default, which stores the body as HTML.
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        return extension is ".md" or ".markdown" ? Document360Enums.EditorFormatMarkdown : null;
    }

    private static List<Dictionary<string, object>> BuildCustomFields(IEnumerable<string>? fieldIds,
        IEnumerable<string>? values)
    {
        var ids = fieldIds?.ToList() ?? [];
        var fieldValues = values?.ToList() ?? [];

        if (ids.Count == 0 && fieldValues.Count == 0) return [];

        if (ids.Count != fieldValues.Count)
            throw new PluginMisconfigurationException(
                $"'Custom field IDs' holds {ids.Count} entries but 'Custom field values' holds {fieldValues.Count}. " +
                "Give exactly one value for each custom field.");

        return ids
            .Select((id, index) => new Dictionary<string, object>
            {
                ["field_id"] = id,
                ["value"] = fieldValues[index]
            })
            .ToList();
    }

    private static int? ParseOption(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (!int.TryParse(value, out var parsed))
            throw new PluginMisconfigurationException(
                $"'{value}' is not a valid choice for '{fieldName}'. Please pick one of the offered options.");

        return parsed;
    }

    private static void RequireDescriptionFits(string? description)
    {
        if (description is null || description.Length <= DescriptionMaxLength) return;

        throw new PluginMisconfigurationException(
            $"The article description is {description.Length} characters long, but Document360 allows at most " +
            $"{DescriptionMaxLength}. Please shorten it.");
    }

    private static void RequireArticle(ArticleIdentifier identifier)
    {
        RequireWorkspace(identifier);
        Require(identifier.ArticleId, "Article ID");
        Require(identifier.Locale, "Locale");
    }

    private static void RequireWorkspace(WorkspaceIdentifier identifier) =>
        Require(identifier.WorkspaceId, "Workspace ID");

    private static void Require(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new PluginMisconfigurationException($"'{fieldName}' cannot be empty. Please fill it in.");
    }

    #endregion
}
