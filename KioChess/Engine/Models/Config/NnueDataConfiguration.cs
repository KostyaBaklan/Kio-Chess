namespace Engine.Models.Config;

public class NnueDataConfiguration
{
    public string SourceDirectory { get; set; }
    public string OutputDirectory { get; set; }
    public string FilteredFile { get; set; }
    public NnueFilterConfiguration Filter { get; set; }
    public NnueExtractConfiguration Extract { get; set; }
    public NnueLabelConfiguration Label { get; set; }
    public NnueMergeConfiguration Merge { get; set; }
}
