using Numan.Config;
using Numan.Models;
using Numan.Utils;
using Spectre.Console;

namespace Numan.Commands;

public static class InitCommand
{
    public static void Execute()
    {
        AnsiConsole.MarkupLine("[blue]Initializing numan...[/]");
        var sources = NuGetUtils.DetectNuGetSources();

        var config = ConfigManager.Config;
        config.NugetSources = sources;

        foreach (var source in sources)
        {
            AnsiConsole.MarkupLine($"[blue]New local source found: {source.Name} - {source.Value}[/]");
        }

        if (sources.Count > 1)
        {
            AnsiConsole.MarkupLine($"[yellow]Numan has detected multiple local NuGet sources. A default local source is needed to install new packages to when no source is specified with the 'add' command.[/]");
            var defaultSource = AnsiConsole.Prompt(
                new SelectionPrompt<NugetSource>()
                    .Title("[yellow]Please select your preferred default source:[/]")
                    .PageSize(10)
                    .MoreChoicesText("[gray](Move up and down to reveal more packages)[/]")
                    .AddChoices(sources)
                    .UseConverter(p => $"{p.Name} {p.Value}")
            );

            defaultSource.IsDefault = true;
            AnsiConsole.MarkupLine($"[blue]Source '{defaultSource.Name}' will be used as default source.[/]");
        } 

        ConfigManager.SaveConfig();

        AnsiConsole.MarkupLine("[green]Numan initialized![/]");
        AnsiConsole.MarkupLine($"[yellow]Config saved at: {ConfigManager.ConfigFilePath}[/]");
    }
}
