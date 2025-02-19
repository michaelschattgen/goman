using System.Text.Json;
using Numan.Config;
using Spectre.Console;
using Spectre.Console.Json;

namespace Numan.Commands;

public class ShowConfigCommand : BaseCommand
{
    public void Execute()
    {
        PreExecute();

        var config = ConfigManager.Config;

        try
        {
            string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var json = new JsonText(jsonString);

            AnsiConsole.Write(new Panel(json).Header("Config JSON").Border(BoxBorder.Rounded));
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error loading config: {ex.Message}[/]");
        }
    }
}
