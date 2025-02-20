using System.Diagnostics;
using Numan.Config;
using Numan.Models;
using Spectre.Console;

namespace Numan.Commands;

public class AddPackageCommand : BaseCommand
{
    public void Execute(string? packagePath, string sourceName, string configuration = "Release")
    {
        PreExecute();

        if (string.IsNullOrWhiteSpace(packagePath))
        {
            packagePath = FindLatestNuGetPackage(configuration);
            if (string.IsNullOrWhiteSpace(packagePath))
            {
                AnsiConsole.MarkupLine($"[red]Error: No .nupkg file found in bin/{configuration}.[/]");
                return;
            }

            string fileName = Path.GetFileName(packagePath);
            AnsiConsole.MarkupLine($"[cyan]Automatically detected package:[/] {fileName}");
        }

        if (!File.Exists(packagePath))
        {
            AnsiConsole.MarkupLine($"[red]Error: Package '{packagePath}' does not exist[/]");
            return;
        }

        var config = ConfigManager.Config;
        NugetSource? source;

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            if (config.NugetSources.Count > 1)
            {
                AnsiConsole.MarkupLine($"[red]Please specify a source.[/]");
                return;
            }

            source = config.NugetSources.FirstOrDefault();
        }
        else
        {
            source = config.NugetSources.Find(s => s.Name != null && s.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                AnsiConsole.MarkupLine($"[red]Error: NuGet source '{sourceName}' not found.[/]");
                return;
            }
        }

        if (source == null)
        {
            AnsiConsole.MarkupLine($"[red]No source found to push the package to.[/]");
            return;
        }


        if (!AddPackageToNuGet(packagePath, source.Value))
        {
            return;
        }

        string packageFolder = Path.GetDirectoryName(packagePath) ?? string.Empty;

        if (!config.MonitoredFolders.Contains(packageFolder))
        {
            config.MonitoredFolders.Add(packageFolder);
            ConfigManager.SaveConfig();

            AnsiConsole.MarkupLine($"[green]Added '{packageFolder}' to monitored folders.[/]");
        }
    }

    private static string? FindLatestNuGetPackage(string configuration)
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "bin", configuration);
        if (!Directory.Exists(path)) return null;

        return Directory.GetFiles(path, "*.nupkg").MaxBy(File.GetLastWriteTime);
    }

    private static bool AddPackageToNuGet(string packagePath, string sourcePath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"nuget push \"{packagePath}\" --source \"{sourcePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                string relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), packagePath);
                AnsiConsole.MarkupLine($"[green]Successfully added package:[/] {relativePath}");
                return true;
            }

            AnsiConsole.MarkupLine($"[red]Failed to add package:[/] {error}");
            return false;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error running 'dotnet nuget add': {ex.Message}[/]");
            return false;
        }
    }
}
