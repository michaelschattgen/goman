using System.CommandLine;
using numan.Commands;
using Numan.Commands;

var rootCommand = new RootCommand("numan - NuGet Package Manager");

var initCommand = new Command("init", "Init numan (run this the first time you start using numan)");
initCommand.SetHandler(InitCommand.Execute);
rootCommand.AddCommand(initCommand);

var listSourcesCommand = new Command("list-sources", "List saved NuGet sources that numan keeps track of");
listSourcesCommand.SetHandler(ListSourcesCommand.Execute);
rootCommand.AddCommand(listSourcesCommand);

rootCommand.Invoke(args);
