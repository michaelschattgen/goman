using Numan.Config;
using Spectre.Console;

namespace Numan.Commands;

public class SetDefaultSourceCommand : BaseCommand
{
    public void Execute()
    {
        var config = ConfigManager.Config;
        var oldDefaultSource = config.NugetSources.FirstOrDefault(x => x.IsDefault);

        if (config.NugetSources.Count <= 1)
        {
            AnsiConsole.MarkupLine("[yellow]You need more than one NuGet source to change the default.[/]");
            return;
        }

        var selectedSource = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[blue]Please select a new default NuGet source[/]: [yellow](previous: {(oldDefaultSource != null ? oldDefaultSource.Value : "")})[/]")
                .PageSize(10)
                .AddChoices(config.NugetSources.Select(s => $"{s.Name} ({s.Value})"))
        );

        var newDefaultSource = config.NugetSources.FirstOrDefault(s => selectedSource.StartsWith(s.Name ?? s.Value, StringComparison.OrdinalIgnoreCase));

        if (newDefaultSource != null)
        {
            newDefaultSource.IsDefault = true;
            if (oldDefaultSource != null)
            {
                oldDefaultSource.IsDefault = false;
            }

            ConfigManager.SaveConfig();
            AnsiConsole.MarkupLine($"[green]Default NuGet source set to:[/] {newDefaultSource.Name} ({newDefaultSource.Value})");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Error: Unable to set default NuGet source.[/]");
        }
    }

}
