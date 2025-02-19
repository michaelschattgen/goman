using Numan.Config;
using Numan.Utils;
using Spectre.Console;

namespace Numan.Commands;

public class RemovePackagesCommand : BaseCommand
{
    public void Execute(bool deleteAllVersions = false)
    {
        PreExecute();

        var config = ConfigManager.Config;
        if (config.NugetSources.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No local NuGet sources found.[/]");
            return;
        }

        Dictionary<string, List<string>> installedPackages = new();
        foreach (var source in config.NugetSources)
        {
            installedPackages = NuGetUtils.GetInstalledPackages(source.Value, !deleteAllVersions);
        }

        List<(string Name, string Version)> selectedPackages = new();

        if (deleteAllVersions)
        {
            var selectedPackageNames = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("[blue]Select packages to delete (all versions will be removed):[/]")
                    .PageSize(10)
                    .MoreChoicesText("[gray](Move up and down to reveal more packages)[/]")
                    .InstructionsText("[gray](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
                    .AddChoices(installedPackages.Keys.ToArray())
            );

            selectedPackages = selectedPackageNames
                .SelectMany(pkg => installedPackages[pkg].Select(version => (pkg, version)))
                .ToList();
        }
        else
        {
            var packageChoices = installedPackages
                .SelectMany(pkg => pkg.Value.Select(version => $"{pkg.Key} {version}"))
                .ToList();

            selectedPackages = AnsiConsole.Prompt(
                new MultiSelectionPrompt<(string, string)>()
                    .Title("[blue]Select package versions to delete:[/]")
                    .PageSize(10)
                    .MoreChoicesText("[gray](Move up and down to reveal more packages)[/]")
                    .InstructionsText("[gray](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
                    .AddChoices(packageChoices.Select(p =>
                        (p.Split(' ')[0], p.Split(' ')[1])
                    ))
                    .UseConverter(p => $"{p.Item1} {p.Item2}")
            );
        }

        if (selectedPackages.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No packages selected for deletion.[/]");
            return;
        }

        if (!AnsiConsole.Confirm("[red]Are you sure you want to delete the selected packages? This action cannot be undone.[/]"))
        {
            return;
        }

        foreach (var (packageName, packageVersion) in selectedPackages)
        {
            foreach (var source in config.NugetSources)
            {
                string packagePath = Path.Combine(source.Value, $"{packageName}.{packageVersion}.nupkg");

                if (File.Exists(packagePath))
                {
                    try
                    {
                        File.Delete(packagePath);
                        AnsiConsole.MarkupLine($"[green]Deleted:[/] {packageName} {packageVersion}");
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Failed to delete {packageName} {packageVersion}: {ex.Message}[/]");
                    }
                }
            }
        }
    }
}
