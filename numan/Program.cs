﻿using System.CommandLine;
using numan.Commands;
using Numan.Commands;

var rootCommand = new RootCommand("numan - NuGet Package Manager");

var initCommand = new Command("init", "Init numan (run this the first time you start using numan)");
initCommand.SetHandler(InitCommand.Execute);
rootCommand.AddCommand(initCommand);

var addPackageCommand = new Command("add", "Adds and tracks a new NuGet package in a local source");

var packageOption = new Option<string>("package", "Path to the .nupkg file");
var sourceOption = new Option<string>("source", "Name of the NuGet source");
addPackageCommand.AddOption(packageOption);
addPackageCommand.AddOption(sourceOption);
addPackageCommand.SetHandler(
    AddPackageCommand.Execute,
    packageOption,
    sourceOption
);
rootCommand.AddCommand(addPackageCommand);

var updateCommand = new Command("update", "Checks for new package versions and adds them if needed.");
updateCommand.SetHandler(UpdateCommand.Execute);
rootCommand.AddCommand(updateCommand);

var listSourcesCommand = new Command("list-sources", "List saved NuGet sources that numan keeps track of");
listSourcesCommand.SetHandler(ListSourcesCommand.Execute);
rootCommand.AddCommand(listSourcesCommand);

var listPackagesCommand = new Command("list-packages", "Lists installed NuGet packages with their latest versions");
var listPackagesSourceOption = new Option<string>("source", "Name of the NuGet source");
listPackagesCommand.AddOption(listPackagesSourceOption);
listPackagesCommand.SetHandler(ListPackagesCommand.Execute, listPackagesSourceOption);
rootCommand.AddCommand(listPackagesCommand);

rootCommand.Invoke(args);
