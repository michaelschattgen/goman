using System.Text.Json;
using Numan.Models;
using Spectre.Console;

namespace Numan.Config;

public static class ConfigManager
{
    private static readonly string _configFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".numan"
    );

    public static string ConfigFilePath => _configFilePath;

    private static NumanConfig? _config;

    public static NumanConfig Config => _config ??= LoadConfig();

    private static NumanConfig LoadConfig()
    {
        if (!File.Exists(_configFilePath))
        {
            AnsiConsole.MarkupLine("[yellow] No numan configuration file found. Please run 'numan init' first.[/]");
            return new NumanConfig();
        }

        try
        {
            string json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<NumanConfig>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new NumanConfig();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load numan config file[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);

            return new NumanConfig();
        }
    }

    public static void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to save numan conifg file {ex.Message}[/]");
        }
    }
}
