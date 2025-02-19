using Numan.Config;
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
        ConfigManager.SaveConfig();

        foreach (var source in sources)
        {
            AnsiConsole.MarkupLine($"[green]New local source found: {source.Name} - {source.Value}![/]");
        }

        AnsiConsole.MarkupLine("[green]numan initialized![/]");
        AnsiConsole.MarkupLine($"[yellow]Config saved at: {ConfigManager.ConfigFilePath}[/]");
    }
}
