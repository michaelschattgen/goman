using System.CommandLine;
using Numan.Commands;

var rootCommand = new RootCommand("Numan - NuGet Package Manager");

var initCommand = new Command("init", "Init numan (run this the first time you start using numan)");
initCommand.SetHandler(InitCommand.Execute);
rootCommand.AddCommand(initCommand);

var addPackageCommand = new Command("add", "Adds and tracks a new NuGet package in a local source");

var packageOption = new Option<string?>("package", "Path to the .nupkg file");
var sourceOption = new Option<string>("source", "Name of the NuGet source");
var configOption = new Option<string>("-c", () => "Debug", "Specify build configuration to look for (Debug/Release)");
addPackageCommand.AddOption(packageOption);
addPackageCommand.AddOption(sourceOption);
addPackageCommand.AddOption(configOption);
addPackageCommand.SetHandler((string? package, string source, string config) =>
    new AddPackageCommand().Execute(package, source, config),
    packageOption, sourceOption, configOption);

rootCommand.AddCommand(addPackageCommand);

var updateCommand = new Command("update", "Checks for new package versions and adds them if needed.");
var allOption = new Option<bool>("--all", "Automatically add all detected new versions without confirmation");
updateCommand.AddOption(allOption);
updateCommand.SetHandler(new UpdateCommand().Execute, allOption);
rootCommand.AddCommand(updateCommand);

var listSourcesCommand = new Command("list-sources", "List saved NuGet sources that numan keeps track of");
listSourcesCommand.SetHandler(new ListSourcesCommand().Execute);
rootCommand.AddCommand(listSourcesCommand);

var listPackagesCommand = new Command("list-packages", "Lists installed NuGet packages with their latest versions");
var listPackagesSourceOption = new Option<string>("source", "Name of the NuGet source");
listPackagesCommand.AddOption(listPackagesSourceOption);
listPackagesCommand.SetHandler(new ListPackagesCommand().Execute, listPackagesSourceOption);
rootCommand.AddCommand(listPackagesCommand);

rootCommand.Invoke(args);