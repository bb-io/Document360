namespace Apps.Document360.Models.Polling;


public class PollingMemory
{
    public DateTime? LastPollingTime { get; set; }
}


public class PublishedVersionsMemory : PollingMemory
{
    public Dictionary<string, int>? PublishedVersions { get; set; }
}
