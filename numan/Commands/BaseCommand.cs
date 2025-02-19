using Numan.Config;
using Spectre.Console;

namespace Numan.Commands;

public abstract class BaseCommand
{
    protected void PreExecute()
    {
        if (GetType() != typeof(InitCommand))
        {
            if (!ConfigManager.ConfigExists())
            {
                AnsiConsole.MarkupLine("[red]Error: No numan configuration found. Run 'numan init' first.[/]");
                Environment.Exit(1);
            }
        }
    }
}
