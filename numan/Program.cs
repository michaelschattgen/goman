using System.CommandLine;
using numan.Commands;

var rootCommand = new RootCommand("numan - NuGet Package Manager");

var initCommand = new Command("init", "Init numan (run this the first time you start using numan)");
initCommand.SetHandler(InitCommand.Execute);
rootCommand.AddCommand(initCommand);
rootCommand.Invoke(args);
