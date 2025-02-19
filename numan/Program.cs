using System.CommandLine;
using Spectre.Console;

var rootCommand = new RootCommand("numan - NuGet Package Manager");

var updateCommand = new Command("test", "Test if this new cli tool works properly");
updateCommand.SetHandler(() =>
{
    AnsiConsole.MarkupLine("[green]it works 😎[/]");
});

rootCommand.AddCommand(updateCommand);
rootCommand.Invoke(args);
