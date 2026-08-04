using Apps.Document360.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Document360.Models.Requests;

public class DownloadArticleOptions
{
    [Display("Use published version",
        Description = "Turn on to download the version that is live on the site instead of the newest draft.")]
    public bool? UsePublishedVersion { get; set; }

    [Display("Version number",
        Description =
            "Download one specific version from the history. Cannot be combined with 'Use published version'.")]
    [DataSource(typeof(ArticleContentVersionDataHandler))]
    public int? VersionNumber { get; set; }

    [Display("Resolve merge codes",
        Description =
            "Turn on to replace snippets, variables and glossary codes with their values. Leave off when the file " +
            "will be translated and sent back, so the codes survive the round trip.")]
    public bool? ResolveMergeCodes { get; set; }

    [Display("File name",
        Description = "A name for the file. Left empty, it is built from the article title and the locale.")]
    public string? FileName { get; set; }
}
