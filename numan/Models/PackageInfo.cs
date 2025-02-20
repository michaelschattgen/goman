using NuGet.Versioning;

namespace Numan.Models;

public class PackageInfo
{
    public string Name { get; }
    public List<NuGetVersion> Versions { get; } = new();
    public string Source { get; }
    public string PackagePath { get; }

    public PackageInfo(string name, string source, string packagePath, IEnumerable<string> versions)
    {
        Name = name;
        Source = source;
        PackagePath = packagePath;
        Versions = versions.Select(NuGetVersion.Parse).OrderByDescending(v => v).ToList();
    }

    public NuGetVersion? GetLatestVersion() => Versions.FirstOrDefault();

    public bool HasNewerVersionThan(NuGetVersion version) => Versions.Any(v => v > version);

    public bool ContainsVersion(NuGetVersion version) => Versions.Contains(version);

    public string GetPackageFilePath(NuGetVersion version) => Path.Combine(PackagePath, $"{Name}.{version}.nupkg");

    public override string ToString() => $"{Name} ({Source}) - Versions: {string.Join(", ", Versions)}";
}
