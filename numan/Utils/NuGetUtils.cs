using System;
using System.Xml;
using numan.Models;

namespace numan.Utils;

public static class NuGetUtils
{
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
}
