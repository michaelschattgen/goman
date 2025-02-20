using System.Text.RegularExpressions;
using System.Xml;
using NuGet.Versioning;
using Numan.Models;

namespace Numan.Utils;

public static class NuGetUtils
{
    public static Regex PackageFileRegex = new Regex(@"^(?<name>.*)\.(?<version>\d+\.\d+\.\d+(-[a-zA-Z0-9-.]+)?)\.nupkg$", RegexOptions.Compiled);

    public static List<NugetSource> DetectNuGetSources()
    {
        List<NugetSource> sources = new();
        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "NuGet", "NuGet.Config");

        if (!File.Exists(configPath))
        {
            Console.WriteLine("[yellow]NuGet.config not found, skipping detection.[/]");
            return sources;
        }

        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configPath);
            XmlNodeList? sourceNodes = doc.SelectNodes("//configuration/packageSources/add");

            if (sourceNodes != null)
            {
                foreach (XmlNode node in sourceNodes)
                {
                    string? name = node.Attributes?["key"]?.InnerText;
                    string? value = node.Attributes?["value"]?.InnerText;
                    if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            sources.Add(new NugetSource(value, name));
                        }
                        else
                        {
                            sources.Add(new NugetSource(value));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[red]Failed to parse NuGet.config: {ex.Message}[/]");
        }

        return sources;
    }

    public static List<PackageInfo> GetInstalledPackages(string sourcePath, bool includeAllVersions = false)
    {
        var packages = new Dictionary<string, PackageInfo>();
        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"[red]Error: NuGet source folder not found: {sourcePath}[/]");
            return new List<PackageInfo>();
        }

        var packageFiles = Directory.GetFiles(sourcePath, "*.nupkg", SearchOption.AllDirectories);
        foreach (var file in packageFiles)
        {
            string fileName = Path.GetFileName(file);
            string absolutePath = Path.GetFullPath(file);
            var match = PackageFileRegex.Match(fileName);

            if (match.Success)
            {
                string packageName = match.Groups[2].Value;
                string packageVersion = match.Groups[3].Value;

                if (!packages.ContainsKey(packageName))
                {
                    packages[packageName] = new PackageInfo(packageName, sourcePath, sourcePath, new List<string>());
                }

                packages[packageName].Versions.Add(NuGetVersion.Parse(packageVersion));
            }
        }

        foreach (var package in packages.Values)
        {
            package.Versions.Sort((a, b) => b.CompareTo(a));
        }

        if (!includeAllVersions)
        {
            return packages.Values
                .Select(p => new PackageInfo(p.Name, p.Source, p.PackagePath, new List<string> { p.GetLatestVersion()?.ToString() ?? "0.0.0" }))
                .ToList();
        }

        return packages.Values.ToList();
    }
}
