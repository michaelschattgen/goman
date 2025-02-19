using System;
using System.Text.Json;
using numan.Utils;
using Spectre.Console;

namespace numan.Commands;

public static class InitCommand
{
    private static readonly string ConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".numan");

    public static void Execute()
    {
        AnsiConsole.MarkupLine("[blue]Initializing numan...[/]");
        var sources = NuGetUtils.DetectNuGetSources();

        var config = new
        {
            nugetSources = sources,
            monitoredFolders = new List<string>()
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        File.WriteAllText(ConfigFilePath, json);

        foreach (var source in sources)
        {
            AnsiConsole.MarkupLine($"[green]New local source found: {source.Name} - {source.Value}![/]");
        }

        AnsiConsole.MarkupLine("[green]numan initialized![/]");
        AnsiConsole.MarkupLine($"[yellow]Config saved at: {ConfigFilePath}[/]");
    }
}
