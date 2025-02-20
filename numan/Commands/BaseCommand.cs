using Numan.Config;
using Spectre.Console;

namespace Numan.Commands;

public abstract class BaseCommand
{
    protected void PreExecute(string? sourceName = null)
    {
        if (GetType() != typeof(InitCommand))
        {
            if (!ConfigManager.ConfigExists())
            {
                AnsiConsole.MarkupLine("[red]Error: No numan configuration found. Run 'numan init' first.[/]");
                Environment.Exit(1);
            }
        }

        if (!string.IsNullOrEmpty(sourceName))
        {
            if (!ConfigManager.Config.NugetSources.Any(x => x.Name == sourceName || x.Value == sourceName))
            {
                AnsiConsole.MarkupLine("[red]Error: Specified source is not found.[/]");
                Environment.Exit(1);
            }
        }
    }
}
