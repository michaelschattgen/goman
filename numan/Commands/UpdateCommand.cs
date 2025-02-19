using Numan.Config;
using Numan.Utils;
using Spectre.Console;

namespace Numan.Commands;

public class UpdateCommand : BaseCommand
{
    public void Execute(bool autoAccept = false, bool allowSelection = false)
    {
        PreExecute();

        var config = ConfigManager.Config;

        if (config.NugetSources.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No NuGet sources configured. Run 'numan init' first.[/]");
            return;
        }

        if (config.MonitoredFolders.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No monitored folders configured. Use 'numan add' to track package folders.[/]");
            return;
        }

        Dictionary<string, string> latestPackages = new();

        foreach (var source in config.NugetSources)
        {
            var installedPackages = NuGetUtils.GetInstalledPackages(source.Value);
            foreach (var package in installedPackages)
            {
                if (!latestPackages.ContainsKey(package.Key.ToLower()) || string.Compare(package.Value, latestPackages[package.Key.ToLower()], StringComparison.OrdinalIgnoreCase) > 0)
                {
                    latestPackages[package.Key.ToLower()] = package.Value;
                }
            }
        }

        var newPackages = new List<(string Name, string Version, string Path)>();

        foreach (var folder in config.MonitoredFolders)
        {
            if (!Directory.Exists(folder)) continue;

            var packageFiles = Directory.GetFiles(folder, "*.nupkg");
            foreach (var file in packageFiles)
            {
                string fileName = Path.GetFileName(file);
                var match = NuGetUtils.PackageFileRegex.Match(fileName);

                if (match.Success)
                {
                    string packageName = match.Groups[1].Value;
                    string packageVersion = match.Groups[2].Value;

                    if (!latestPackages.ContainsKey(packageName.ToLower()) || string.Compare(packageVersion, latestPackages[packageName.ToLower()], StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        newPackages.Add((packageName, packageVersion, file));
                    }
                }
            }
        }

        if (newPackages.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]All packages are up to date![/]");
            return;
        }

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[cyan]Package[/]");
        table.AddColumn("[green]New Version[/]");
        table.AddColumn("[yellow]Current Version[/]");

        foreach (var (name, version, path) in newPackages)
        {
            table.AddRow(
                $"[cyan]{name}[/]",
                $"[green]{version}[/]",
                latestPackages.ContainsKey(name.ToLower()) ? $"[yellow]{latestPackages[name.ToLower()]}[/]" : "[gray]Not Installed[/]"
            );
        }

        AnsiConsole.Write(table);

        List<(string Name, string Version, string Path)> selectedPackages = new();

        if (allowSelection)
        {
            var packageChoices = newPackages
                .Select(p => new SelectionPrompt<(string, string, string)>()
                    .Title("Select packages to add (use space to select, Enter to confirm):")
                    .PageSize(10)
                    .MoreChoicesText("[gray](Move up and down to reveal more packages)[/]")
                    .AddChoices(newPackages)
                    .UseConverter(p => $"{p.Item1} {p.Item2}")
                ).ToList();

            selectedPackages = AnsiConsole.Prompt(new MultiSelectionPrompt<(string, string, string)>()
                .Title("[blue]Select packages to add (use space to select, Enter to confirm):[/]")
                .PageSize(10)
                .MoreChoicesText("[gray](Move up and down to reveal more packages)[/]")
                .InstructionsText("[gray](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
                .AddChoices(newPackages)
                .UseConverter(p => $"{p.Item1} {p.Item2}")
            );
        }
        else
        {
            selectedPackages = newPackages;
        }

        if (!autoAccept && !allowSelection && !AnsiConsole.Confirm("[blue]Do you want to add these new versions to the NuGet source?[/]"))
        {
            return;
        }

        foreach (var (name, version, path) in selectedPackages)
        {
            var source = config.NugetSources.FirstOrDefault();
            if (source != null)
            {
                new AddPackageCommand().Execute(path, source.Name ?? source.Value);
            }
        }
    }
}
