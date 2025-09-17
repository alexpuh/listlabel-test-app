namespace ListLabel.LegacyLabels.Configuration;

public class LegacyLabelsMongoRepositoryOptions
{
    public const string Section = "Repository";
    public string? ConnectionString { get; set; }
    public string? Database { get; set; }
}