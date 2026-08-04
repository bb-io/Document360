namespace Apps.Document360.Models.Polling;

/// <summary>
/// Publishing does not move an article's timestamps in a way that can be filtered on, so the poll remembers the
/// version that was live for every article and compares against it on the next run.
/// </summary>
public class ArticlePublishedMemory
{
    public DateTime? LastPollingTime { get; set; }

    public Dictionary<string, int>? PublishedVersions { get; set; }
}
