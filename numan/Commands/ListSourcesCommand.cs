using Spectre.Console;
using Numan.Config;

namespace Numan.Commands
{
    public static class ListSourcesCommand
    {
        public static void Execute()
        {
            var config = ConfigManager.Config;

            try
            {
                if (config?.NugetSources == null || config.NugetSources.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No NuGet sources found.[/]");
                    return;
                }

                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.AddColumn("[cyan]Name[/]");
                table.AddColumn("[green]Path[/]");

                foreach (var source in config.NugetSources)
                {
                    table.AddRow($"[cyan]{source.Name}[/]", $"[green]{source.Value}[/]");
                }

                AnsiConsole.Write(table);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error loading NuGet sources: {ex.Message}[/]");
            }
        }
    }
}
