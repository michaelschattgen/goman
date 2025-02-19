namespace Numan.Models;

public class PackageInfo
{
    public string Name { get; }
    public List<Version> Versions { get; } = new();
    public string Source { get; }
    public string PackagePath { get; }

    public PackageInfo(string name, string source, string packagePath, IEnumerable<string> versions)
    {
        Name = name;
        Source = source;
        PackagePath = packagePath;
        Versions = versions.Select(Version.Parse).OrderByDescending(v => v).ToList();
    }

    public Version? GetLatestVersion() => Versions.FirstOrDefault();

    public bool HasNewerVersionThan(Version version) => Versions.Any(v => v > version);

    public bool ContainsVersion(Version version) => Versions.Contains(version);

    public string GetPackageFilePath(Version version) => Path.Combine(PackagePath, $"{Name}.{version}.nupkg");

    public override string ToString() => $"{Name} ({Source}) - Versions: {string.Join(", ", Versions)}";
}
