using numan.Config;
using numan.Utils;
using Spectre.Console;

namespace numan.Commands;

public static class UpdateCommand
{
    public static void Execute()
    {
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
                if (!latestPackages.ContainsKey(package.Key) || string.Compare(package.Value, latestPackages[package.Key], StringComparison.OrdinalIgnoreCase) > 0)
                {
                    latestPackages[package.Key.ToLower()] = package.Value.ToLower();
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

                    if (!latestPackages.ContainsKey(packageName.ToLower()) || string.Compare(packageVersion, latestPackages[packageName], StringComparison.OrdinalIgnoreCase) > 0)
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
                latestPackages.ContainsKey(name) ? $"[yellow]{latestPackages[name]}[/]" : "[gray]Not Installed[/]"
            );
        }

        AnsiConsole.Write(table);

        if (AnsiConsole.Confirm("[blue]Do you want to add these new versions to the NuGet source?[/]"))
        {
            foreach (var (name, version, path) in newPackages)
            {
                var source = config.NugetSources.FirstOrDefault();
                if (source != null)
                {
                    AddPackageCommand.Execute(path, source.Name ?? source.Value);
                }
            }
        }
    }
}
