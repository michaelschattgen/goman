namespace Numan.Models;

public class NugetSource
{
    public string? Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public NugetSource()
    {

    }

    public NugetSource(string value, string name)
    {
        Value = value;
        Name = name;
    }

    public NugetSource(string value)
    {
        Value = value;
    }
}