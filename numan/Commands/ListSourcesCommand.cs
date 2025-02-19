using System.Text.Json;
using Spectre.Console;
using numan.Models;

namespace Numan.Commands
{
    public static class ListSourcesCommand
    {
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".numan"
        );

        public static void Execute()
        {
            if (!File.Exists(ConfigFilePath))
            {
                AnsiConsole.MarkupLine("[red]No NuMan configuration found. Run 'numan init' first.[/]");
                return;
            }

            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<NumanConfig>(json, new JsonSerializerOptions() {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

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
