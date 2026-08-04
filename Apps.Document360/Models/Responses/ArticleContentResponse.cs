using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Document360.Models.Responses;


public class ArticleContentResponse : IDownloadContentOutput
{
    [Display("File")] public FileReference Content { get; set; } = default!;

    [Display("Article ID")] public string ArticleId { get; set; } = string.Empty;

    [Display("Locale")] public string Locale { get; set; } = string.Empty;
}
