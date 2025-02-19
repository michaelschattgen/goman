using System.Text.RegularExpressions;
using System.Xml;
using Numan.Models;

namespace Numan.Utils;

public static class NuGetUtils
{
    public static Regex PackageFileRegex = new Regex(@"^(.*)\.(\d+\.\d+\.\d+(-[a-zA-Z0-9]+)?)\.nupkg$", RegexOptions.Compiled);

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

    public static Dictionary<string, string> GetInstalledPackages(string sourcePath)
    {
        var packageVersions = new Dictionary<string, List<string>>();

        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine($"[red]Error: NuGet source folder not found: {sourcePath}[/]");
            return new Dictionary<string, string>();
        }

        var packageFiles = Directory.GetFiles(sourcePath, "*.nupkg");
        foreach (var file in packageFiles)
        {
            string fileName = Path.GetFileName(file);
            var match = PackageFileRegex.Match(fileName);

            if (match.Success)
            {
                string packageName = match.Groups[1].Value;
                string packageVersion = match.Groups[2].Value;

                if (!packageVersions.ContainsKey(packageName))
                {
                    packageVersions[packageName] = new List<string>();
                }

                packageVersions[packageName].Add(packageVersion);
            }
        }

        return packageVersions.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderByDescending(v => v, StringComparer.OrdinalIgnoreCase).First()
        );
    }
}
