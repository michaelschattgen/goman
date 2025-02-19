using System.Diagnostics;
using numan.Config;
using numan.Models;
using numan.Utils;
using Spectre.Console;

namespace numan.Commands;

public static class ListPackagesCommand
{
    public static void Execute(string sourceName)
    {
        var config = ConfigManager.Config;
        NugetSource? source;

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            if (config.NugetSources.Count > 1)
            {
                AnsiConsole.MarkupLine($"[red]Please specify a source.[/]");
                return;
            }

            source = config.NugetSources.FirstOrDefault();
        }
        else
        {
            source = config.NugetSources.Find(s => s.Name != null && s.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                AnsiConsole.MarkupLine($"[red]Error: NuGet source '{sourceName}' not found.[/]");
                return;
            }
        }

        if (source == null)
        {
            AnsiConsole.MarkupLine($"[red]No sources found.[/]");
            return;
        }

        Dictionary<string, string> latestPackages = new();
        var installedPackages = NuGetUtils.GetInstalledPackages(source.Value);
        foreach (var package in installedPackages)
        {
            if (!latestPackages.ContainsKey(package.Key) || string.Compare(package.Value, latestPackages[package.Key], StringComparison.OrdinalIgnoreCase) > 0)
            {
                latestPackages[package.Key] = package.Value;
            }

        }

        if (latestPackages.Count == 0)
        {
            AnsiConsole.Markup("[yellow]No installed packages found.[/]");
            return;
        }

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[cyan]Package[/]");
        table.AddColumn("[green]Latest Version[/]");

        foreach (var package in latestPackages)
        {
            table.AddRow($"[cyan]{package.Key}[/]", $"[green]{package.Value}[/]");
        }

        AnsiConsole.Write(table);
    }
}
