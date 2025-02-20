using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NuGet.Versioning;
using Numan.Models;
using Spectre.Console;

namespace Numan.Utils;

public static class NuGetUtils
{
    private static readonly XNamespace ns = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd";
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
        var config = Config.ConfigManager.Config;

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"NuGet source directory not found: {sourcePath}");

        bool isHierarchical = Directory.GetDirectories(sourcePath).Any();
        if (config.NugetSources.Any(x => x.Name == sourcePath || x.Value == sourcePath) && isHierarchical)
        {
            foreach (var packageDir in Directory.GetDirectories(sourcePath))
            {
                string packageName = Path.GetFileName(packageDir);
                foreach (var versionDir in Directory.GetDirectories(packageDir))
                {
                    NuGetVersion version = NuGetVersion.Parse(Path.GetFileName(versionDir));

                    string? nuspecPath = Directory.GetFiles(versionDir, "*.nuspec").FirstOrDefault();
                    if (!string.IsNullOrEmpty(nuspecPath))
                    {
                        var (id, ver) = ParseNuspec(nuspecPath);
                        if (!string.IsNullOrEmpty(id))
                        {
                            packageName = id;
                            version = ver;
                        }
                    }

                    string? nupkgPath = Directory.GetFiles(versionDir, "*.nupkg").FirstOrDefault();
                    if (!string.IsNullOrEmpty(nupkgPath))
                    {
                        if (!packages.ContainsKey(packageName))
                        {
                            packages[packageName] = new PackageInfo(packageName, sourcePath, nupkgPath, new List<string>());
                        }
                        packages[packageName].Versions.Add(version);
                    }
                }
            }
        }
        else
        {
            foreach (var nupkgPath in Directory.GetFiles(sourcePath, "*.nupkg"))
            {
                string fileName = Path.GetFileName(nupkgPath);
                string absolutePath = Path.GetFullPath(nupkgPath);
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
        }

        return includeAllVersions
                    ? packages.Values.ToList()
                    : packages.Values
                        .Select(p => new PackageInfo(
                            p.Name,
                            p.Source,
                            p.PackagePath,
                            new List<string> { p.GetLatestVersion()?.ToString() ?? "0.0.0" }))
                        .ToList();
    }

    private static (string id, NuGetVersion version) ParseNuspec(string nuspecPath)
    {
        try
        {
            XDocument doc = XDocument.Load(nuspecPath);
            XElement? metadata = doc.Root?.Element(ns + "metadata");

            if (metadata != null)
            {
                string id = metadata.Element(ns + "id")?.Value ?? string.Empty;
                string versionString = metadata.Element(ns + "version")?.Value ?? string.Empty;
                NuGetVersion version = NuGetVersion.Parse(versionString);
                return (id, version);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading .nuspec: {nuspecPath} - {ex.Message}");
        }
        return (string.Empty, new NuGetVersion("0.0.0"));
    }
}