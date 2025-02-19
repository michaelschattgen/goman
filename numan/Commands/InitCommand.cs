using System;
using System.Text.Json;
using numan.Config;
using numan.Utils;
using Spectre.Console;

namespace numan.Commands;

public static class InitCommand
{
    public static void Execute()
    {
        AnsiConsole.MarkupLine("[blue]Initializing numan...[/]");
        var sources = NuGetUtils.DetectNuGetSources();

        var config = ConfigManager.Config;
        config.NugetSources = sources;
        ConfigManager.SaveConfig();

        foreach (var source in sources)
        {
            AnsiConsole.MarkupLine($"[green]New local source found: {source.Name} - {source.Value}![/]");
        }

        AnsiConsole.MarkupLine("[green]numan initialized![/]");
        AnsiConsole.MarkupLine($"[yellow]Config saved at: {ConfigManager.ConfigFilePath}[/]");
    }
}
