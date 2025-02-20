using Numan.Config;
using Numan.Models;
using Numan.Utils;
using Spectre.Console;

namespace Numan.Commands;

public class ListPackagesCommand : BaseCommand
{
    public void Execute(string sourceName)
    {
        PreExecute(sourceName);

        var config = ConfigManager.Config;
        NugetSource? source = null;

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            if (config.NugetSources.Count > 1)
            {
                var defaultSource = ConfigManager.GetDefaultSource();
                sourceName = defaultSource.Name ?? defaultSource.Value;
                AnsiConsole.MarkupLine($"[yellow]Multiple local NuGet sources found, using default source ({sourceName}). You can override this by using the --source parameter.[/]");
            }

            source = config.NugetSources.FirstOrDefault();
        }
        else
        {
            source = config.NugetSources.FirstOrDefault(x => x.Name == sourceName || x.Value == sourceName);
        }

        if (source == null)
        {
            AnsiConsole.MarkupLine($"[red]No sources found.[/]");
            return;
        }

        var latestPackages = NuGetUtils.GetInstalledPackages(source.Value);
        if (latestPackages.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No installed packages found.[/]");
            return;
        }

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[cyan]Package[/]");
        table.AddColumn("[green]Latest Version[/]");

        foreach (var package in latestPackages)
        {
            table.AddRow($"[cyan]{package.Name}[/]", $"[green]{package.Versions.First()}[/]");
        }

        AnsiConsole.Write(table);
    }
}
