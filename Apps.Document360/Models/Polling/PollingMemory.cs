namespace Apps.Document360.Models.Polling;

/// <summary>What a poll remembers so the next run can tell a change from something that was already there.</summary>
public class PollingMemory
{
    public DateTime? LastPollingTime { get; set; }
}

/// <summary>
/// Publishing does not move an article's timestamps in a way that can be filtered on, so the poll remembers the
/// version that was live for every article and compares against it on the next run.
/// </summary>
public class PublishedVersionsMemory : PollingMemory
{
    public Dictionary<string, int>? PublishedVersions { get; set; }
}
