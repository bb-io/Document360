using System.Net.Mime;
using System.Text;
using Apps.Document360.Api;
using Apps.Document360.Constants;
using Apps.Document360.Extensions;
using Apps.Document360.Models.Dtos;
using Apps.Document360.Models.Entities;
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
using RestSharp;

namespace Apps.Document360.Actions;

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
        Validate.Workspace(request);
        Validate.Required(request.Title, "Title");
        Validate.Required(request.CategoryId, "Category ID");
        Validate.Required(request.UserId, "User ID");

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

        body.AddIfNotNull("content_type", ResolveEditorFormat(request.EditorFormat, request.File.Name))
            .AddIfNotEmpty("slug", request.Slug)
            .AddIfNotNull("order", request.Order);

        var created = await CreateArticleAsync(body);

        var locale = string.IsNullOrWhiteSpace(created.LanguageCode)
            ? await ResolveDefaultLocaleAsync(request.WorkspaceId)
            : created.LanguageCode;

        var article = await GetArticleAsync(created.Id, locale);
        return new CreateArticleResponse(article, locale);
    }

    [Action("Update article",
        Description =
            "Update the content and metadata of an article translation, leaving it as an editable draft. " +
            "A locale that has never been translated is created on first use.")]
    public async Task<UpdateArticleResponse> UpdateArticle([ActionParameter] UpdateArticleRequest request)
    {
        Validate.Article(request);

        if (request.File is not null && !string.IsNullOrEmpty(request.Content))
            throw new PluginMisconfigurationException(
                "The article body was supplied twice. Please fill in either 'File' or 'Content', not both.");

        RequireDescriptionFits(request.Description);

        var customFields = BuildCustomFields(request.CustomFieldIds, request.CustomFieldValues);
        var current = await GetArticleAsync(request.ArticleId, request.Locale);

        var contentBody = new Dictionary<string, object>()
            .AddIfNotEmpty("title", request.Title)
            .AddIfNotNull("content",
                request.File is not null ? await ReadFileAsTextAsync(request.File) : request.Content)
            .AddIfNotEmpty("translation_option", request.TranslationStatus)
            .AddIfNotNull("hidden", request.Hidden)
            .AddIfNotEmpty("category_id", request.CategoryId)
            .AddIfNotNull("order", request.Order);

        var wasCopiedToNewDraft =
            await WriteArticleContentAsync(request.ArticleId, request.Locale, contentBody, current, request.UserId);

        if (request.Description is not null)
            await UpdateArticleDescriptionAsync(request.ArticleId, request.Locale, request.Description);

        var settingsBody = new Dictionary<string, object>()
            .AddIfNotEmpty("seo_title", request.SeoTitle)
            .AddIfNotEmpty("description", request.MetaDescription);

        if (request.Tags is not null) settingsBody["tags"] = request.Tags.ToList();
        if (customFields.Count > 0) settingsBody["custom_fields"] = customFields;

        if (settingsBody.Count > 0)
            await UpdateArticleSettingsAsync(request.ArticleId, request.Locale, settingsBody);

        var article = await GetArticleAsync(request.ArticleId, request.Locale);
        var settings = await TryGetSettingsAsync(request.ArticleId, request.Locale);

        return new UpdateArticleResponse(article, settings, request.Locale, wasCopiedToNewDraft);
    }

    [Action("Publish article", Description = "Put an article translation live on the knowledge base site.")]
    public async Task<PublishArticleResponse> PublishArticle([ActionParameter] PublishArticleRequest request)
    {
        Validate.Article(request);
        Validate.Required(request.UserId, "User ID");

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

        var publishResult = await PublishVersionAsync(request.ArticleId, request.Locale, body);
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
        Validate.Workspace(request);
        Validate.Required(request.ContentId, "Article ID");
        Validate.Required(request.Locale, "Locale");

        if (options.VersionNumber.HasValue && options.UsePublishedVersion == true)
            throw new PluginMisconfigurationException(
                "'Version number' and 'Use published version' cannot be combined. Choose one version to download.");

        if (options.VersionNumber is <= 0)
            throw new PluginMisconfigurationException("'Version number' has to be 1 or higher.");

        await RequireLocaleExistsAsync(request.WorkspaceId, request.Locale);

        var article = options.VersionNumber.HasValue
            ? await GetArticleVersionAsync(request.ContentId, request.Locale, options.VersionNumber.Value,
                options.ResolveMergeCodes ?? false)
            : await GetArticleAsync(request.ContentId, request.Locale, options.UsePublishedVersion ?? false,
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
        Validate.Workspace(request);
        Validate.Required(request.Locale, "Locale");

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

        var current = await GetArticleAsync(articleId, request.Locale);
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
            await UpdateArticleDescriptionAsync(articleId, request.Locale, fields.Description!);
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
            await UpdateArticleSettingsAsync(articleId, request.Locale, settingsBody);

        var article = await GetArticleAsync(articleId, request.Locale);

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
        Validate.Article(request);

        var article = await GetArticleAsync(request.ArticleId, request.Locale,
            request.UsePublishedVersion ?? false);
        var settings = await TryGetSettingsAsync(request.ArticleId, request.Locale);

        return new ArticleInfoResponse(article, settings, request.Locale);
    }

    [Action("Search articles", Description = "Find the articles of a workspace that match your filters.")]
    public async Task<SearchArticlesResponse> SearchArticles([ActionParameter] SearchArticlesRequest request)
    {
        Validate.Workspace(request);

        if (!string.IsNullOrWhiteSpace(request.TranslationStatus) && string.IsNullOrWhiteSpace(request.Locale))
            throw new PluginMisconfigurationException(
                "Filtering by translation status needs to know which language to look at. Please fill in 'Locale'.");

        if (request.MaxResults is <= 0)
            throw new PluginMisconfigurationException(
                "'Max results' has to be 1 or higher. Leave it empty to return every match.");

        var articles = await ArticleProvider.GetArticlesAsync(Client, request.WorkspaceId, request.Locale,
            Validate.Option(request.Visibility, "Visibility"));

        if (!string.IsNullOrWhiteSpace(request.TranslationStatus))
        {
            var translationStatus = Validate.Option(request.TranslationStatus, "Translation status")!.Value;
            var matching = await GetTranslationArticleIdsAsync(request.WorkspaceId, request.Locale!,
                translationStatus);
            articles = articles.Where(article => matching.Contains(article.Id)).ToList();
        }

        articles = ArticleFilters.ByPublicationStatus(articles, request.PublicationStatus);

        if (request.Hidden.HasValue)
            articles = articles.Where(x => (x.Hidden ?? false) == request.Hidden.Value).ToList();

        if (request.ModifiedAfter.HasValue)
            articles = ArticleFilters.ModifiedAfter(articles, request.ModifiedAfter.Value);

        if (request.MaxResults.HasValue) articles = articles.Take(request.MaxResults.Value).ToList();

        var results = articles
            .Select(article => new ArticleListItemResponse(article, request.WorkspaceId))
            .ToList();

        return new SearchArticlesResponse { Articles = results, TotalCount = results.Count };
    }


    private Task<ArticleDto> GetArticleAsync(string articleId, string langCode, bool isPublished = false,
        bool isForDisplay = false) =>
        Client.GetDataAsync<ArticleDto>(new RestRequest($"/v2/Articles/{articleId}/{langCode}", Method.Get)
            .AddQueryParameter("isForDisplay", Flag(isForDisplay))
            .AddQueryParameter("isPublished", Flag(isPublished))
            .AddQueryParameter("appendSASToken", "true"));

    private Task<ArticleDto> GetArticleVersionAsync(string articleId, string langCode, int versionNumber,
        bool isForDisplay = false) =>
        Client.GetDataAsync<ArticleDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/versions/{versionNumber}", Method.Get)
                .AddQueryParameter("isForDisplay", Flag(isForDisplay))
                .AddQueryParameter("appendSASToken", "true"));

    private Task<CreatedArticleDto> CreateArticleAsync(Dictionary<string, object> body) =>
        Client.GetDataAsync<CreatedArticleDto>(new RestRequest("/v2/Articles", Method.Post).AddJsonBody(body));


    private async Task<bool> TryUpdateArticleAsync(string articleId, string langCode,
        Dictionary<string, object> body)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}", Method.Put).AddJsonBody(body);
        var envelope = await Client.SendUncheckedAsync<ArticleDto>(request);

        if (envelope.Success) return true;
        if (RequiresNewDraft(envelope.ErrorMessage)) return false;

        throw Client.BuildFailureException(envelope.ErrorMessage);
    }


    private Task<ArticleVersionDto> ForkArticleAsync(string articleId, Dictionary<string, object> body) =>
        Client.GetDataAsync<ArticleVersionDto>(
            new RestRequest($"/v2/Articles/{articleId}/fork", Method.Put).AddJsonBody(body));


    private async Task UpdateArticleDescriptionAsync(string articleId, string langCode, string description)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}/updateDescription", Method.Put)
            .AddQueryParameter("description", description);

        var envelope = await Client.SendUncheckedAsync<object>(request);

        if (!envelope.Success && envelope.HasErrorDetails) throw Client.BuildFailureException(envelope.ErrorMessage);
    }

    private Task<ArticleSettingsDto> UpdateArticleSettingsAsync(string articleId, string langCode,
        Dictionary<string, object> body) =>
        Client.GetDataAsync<ArticleSettingsDto>(
            new RestRequest($"/v2/Articles/{articleId}/{langCode}/settings", Method.Put).AddJsonBody(body));


    private async Task<(bool WasAlreadyLive, string? Url)> PublishVersionAsync(string articleId, string langCode,
        Dictionary<string, object> body)
    {
        var request = new RestRequest($"/v2/Articles/{articleId}/{langCode}/publish", Method.Post).AddJsonBody(body);
        var envelope = await Client.SendUncheckedAsync<object>(request);

        if (envelope.Success) return (false, envelope.Url);
        if (IsAlreadyPublished(envelope.ErrorMessage)) return (true, null);

        throw Client.BuildFailureException(envelope.ErrorMessage);
    }


    private async Task<HashSet<string>> GetTranslationArticleIdsAsync(string workspaceId, string langCode, int status)
    {
        var request = new RestRequest($"/v2/Translations/{workspaceId}/{langCode}", Method.Get)
            .AddQueryParameter("status", status.ToString());

        var entries = await Client.GetListAsync<TranslationEntryDto>(request);
        return entries.Select(entry => entry.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string Flag(bool value) => value ? "true" : "false";

    private static bool RequiresNewDraft(string? message) =>
        message is not null &&
        (message.Contains("fork the article", StringComparison.OrdinalIgnoreCase) ||
         message.Contains("read-only", StringComparison.OrdinalIgnoreCase) ||
         message.Contains("read only", StringComparison.OrdinalIgnoreCase));

    private static bool IsAlreadyPublished(string? message) =>
        message is not null && message.Contains("already been published", StringComparison.OrdinalIgnoreCase);


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

        if (await TryUpdateArticleAsync(articleId, locale, contentBody)) return wasCopiedToNewDraft;

        if (wasCopiedToNewDraft) throw ReadOnlyAfterCopy();

        await CopyToNewDraftAsync(articleId, locale, current, userId);

        if (!await TryUpdateArticleAsync(articleId, locale, contentBody)) throw ReadOnlyAfterCopy();

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

        await ForkArticleAsync(articleId, body);
    }

    private static PluginApplicationException ReadOnlyAfterCopy() =>
        new("Document360 kept the article in read-only mode even after it was copied into a new draft. " +
            "Check the article's workflow status in Document360 and try again.");

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

    private async Task<int> ResolveLatestVersionAsync(string articleId, string locale)
    {
        var versions = await ArticleProvider.GetArticleVersionsAsync(Client, articleId, locale);

        if (versions.Count == 0)
            throw new PluginMisconfigurationException(
                "This article has no version in the chosen locale, so there is nothing to publish. Update the " +
                "article in that locale first.");

        return versions.Max(version => version.VersionNumber);
    }

    private async Task<string> ResolveDefaultLocaleAsync(string workspaceId)
    {
        var locales = await ProjectProvider.GetWorkspaceLocalesAsync(Client, workspaceId);
        var defaultLocale =
            locales.FirstOrDefault(locale => locale.IsSetAsDefault == true) ?? locales.FirstOrDefault();

        return defaultLocale?.LanguageCode ??
               throw new PluginApplicationException(
                   "The article was created, but Document360 reported no language for the workspace, so it could " +
                   "not be read back. Use 'Get article info' to fetch its details.");
    }

    private async Task RequireLocaleExistsAsync(string workspaceId, string locale)
    {
        var locales = await ProjectProvider.GetWorkspaceLocalesAsync(Client, workspaceId);
        if (locales.Any(x => string.Equals(x.LanguageCode, locale, StringComparison.OrdinalIgnoreCase))) return;

        throw new PluginMisconfigurationException(
            $"The locale '{locale}' is not enabled for this workspace. Available locales: " +
            $"{string.Join(", ", locales.Select(x => x.LanguageCode))}.");
    }


    private async Task<ArticleSettingsDto?> TryGetSettingsAsync(string articleId, string locale)
    {
        try
        {
            return await ArticleProvider.GetArticleSettingsAsync(Client, articleId, locale);
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
            return await GetArticleAsync(articleId, locale, isPublished);
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
        if (!string.IsNullOrWhiteSpace(editorFormat)) return Validate.Option(editorFormat, "Editor format");


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

    private static void RequireDescriptionFits(string? description)
    {
        if (description is null || description.Length <= DescriptionMaxLength) return;

        throw new PluginMisconfigurationException(
            $"The article description is {description.Length} characters long, but Document360 allows at most " +
            $"{DescriptionMaxLength}. Please shorten it.");
    }
}
