namespace Numan.Models;

public class NumanConfig
{
    public List<NugetSource> NugetSources { get; set; } = new();
    public List<string> MonitoredFolders { get; set; } = new();
}