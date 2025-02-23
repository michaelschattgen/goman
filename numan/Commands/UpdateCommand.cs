using NuGet.Versioning;
using Numan.Config;
using Numan.Models;
using Numan.Utils;
using Spectre.Console;

namespace Numan.Commands;

public class UpdateCommand : BaseCommand
{
    public void Execute(string? sourceName, bool autoAccept = false, bool allowSelection = false)
    {
        PreExecute();

        var config = ConfigManager.Config;

        if (config.NugetSources.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No NuGet sources configured. Run 'numan init' first.[/]");
            return;
        }

        if (config.NugetSources.Count > 1)
        {
            var defaultSource = ConfigManager.GetDefaultSource();
            sourceName = defaultSource.Name ?? defaultSource.Value;
            AnsiConsole.MarkupLine($"[yellow]Multiple local NuGet sources found, using default source ({sourceName}). You can override this by using the --source parameter.[/]");
        }

        if (config.MonitoredFolders.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No monitored folders configured. Use 'numan add' to track package folders.[/]");
            return;
        }

        List<PackageInfo> installedPackages = new();
        foreach (var source in config.NugetSources.Where(x => x.Name == sourceName))
        {
            installedPackages.AddRange(NuGetUtils.GetInstalledPackages(source.Value, includeAllVersions: false));
        }

        List<PackageInfo> monitoredPackages = new();
        foreach (var folder in config.MonitoredFolders)
        {
            if (!Directory.Exists(folder)) continue;
            monitoredPackages.AddRange(NuGetUtils.GetInstalledPackages(folder, includeAllVersions: true));
        }

        var newPackages = new List<(PackageInfo Package, NuGetVersion NewVersion, string FilePath, bool isInstalled)>();
        foreach (var monitoredPackage in monitoredPackages)
        {
            var installedPackage = installedPackages.FirstOrDefault(p => p.Name.ToLower() == monitoredPackage.Name.ToLower());
            var latestMonitoredVersion = monitoredPackage.GetLatestVersion();

            if (latestMonitoredVersion == null) continue;

            if (installedPackage == null)
            {
                string packageFilePath = monitoredPackage.GetPackageFilePath(latestMonitoredVersion);
                newPackages.Add((monitoredPackage, latestMonitoredVersion, packageFilePath, isInstalled: false));

                AnsiConsole.MarkupLine($"[green]New package detected:[/] {monitoredPackage.Name} {latestMonitoredVersion}");
            }
            else if (latestMonitoredVersion > (installedPackage.GetLatestVersion() ?? new NuGetVersion("0.0.0")))
            {
                string packageFilePath = monitoredPackage.GetPackageFilePath(latestMonitoredVersion);
                newPackages.Add((installedPackage, latestMonitoredVersion, packageFilePath, isInstalled: true));

                AnsiConsole.MarkupLine($"[green]New version detected:[/] {monitoredPackage.Name} {latestMonitoredVersion}");
            }
        }

        if (newPackages.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]All packages are up to date![/]");
            return;
        }

        var table = new Table { Border = TableBorder.Rounded };
        table.AddColumn("[cyan]Package[/]");
        table.AddColumn("[green]New Version[/]");
        table.AddColumn("[yellow]Current Version[/]");

        foreach (var (package, newVersion, _, isInstalled) in newPackages)
        {
            table.AddRow(
                $"[cyan]{package.Name}[/]",
                $"[green]{newVersion}[/]",
                isInstalled ? $"[yellow]{package.GetLatestVersion()}[/]" : "[gray]Not Installed[/]"
            );
        }

        AnsiConsole.Write(table);

        List<(PackageInfo Package, NuGetVersion NewVersion, string FilePath, bool isInstalled)> selectedPackages;

        if (allowSelection)
        {
            selectedPackages = AnsiConsole.Prompt(
                new MultiSelectionPrompt<(PackageInfo, NuGetVersion, string, bool)>()
                    .Title("[blue]Select packages to update (use space to select, Enter to confirm):[/]")
                    .PageSize(10)
                    .MoreChoicesText("[gray](Move up and down to reveal more packages)[/]")
                    .InstructionsText("[gray](Press [blue]<space>[/] to select, [green]<enter>[/] to confirm)[/]")
                    .AddChoices(newPackages)
                    .UseConverter(p => $"{p.Item1.Name} {p.Item2}")
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

        foreach (var (package, newVersion, filePath, isInstalled) in selectedPackages)
        {
            new AddPackageCommand().Execute(filePath, sourceName ?? "");
        }
    }
}
