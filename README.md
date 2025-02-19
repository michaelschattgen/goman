# Numan - NuGet Package Manager

Numan is a CLI tool that simplifies managing local (for now) NuGet packages. It automates tracking, updating, and adding `.nupkg` files to your local NuGet sources, reducing the manual overhead of managing packages in local development environments.

## Installation

### Prerequisites

- [.NET SDK 6.0 or later](https://dotnet.microsoft.com/download/dotnet/6.0)

### Install via .NET Tool

```sh
dotnet tool install --global numan
```

### Build from Source

```sh
git clone https://github.com/michaelschattgen/numan.git
cd numan
dotnet build -c Release
dotnet tool install --global --add-source ./bin/Release numan
```

## Getting Started

### Initialize Numan

Run the following command to set up Numan and detect existing NuGet sources:

```sh
numan init
```

### Add a Package

To add a `.nupkg` package to a local NuGet source, use:

```sh
numan add <package-path>
```

If no package path is specified, NuMan automatically detects `.nupkg` files in `bin/Debug` or `bin/Release` depending on the value of the configuration argument (-c) either 'Debug' or 'Release'.

### List Installed Packages

To list all packages in a NuGet source:

```sh
numan list
```

### List NuGet Sources

To view all tracked local NuGet sources:

```sh
numan list-sources
```

### Update Packages

To check for and add new package versions:

```sh
numan update
```

To update all new versions automatically without prompts:

```sh
numan update --all
```

## Configuration

Numan stores configuration in `~/.numan/config.json`. This file tracks monitored package folders and NuGet sources. You can manually edit it if needed.

## Contributing

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature-xyz`
3. Commit your changes: `git commit -m "Add feature xyz"`
4. Push to the branch: `git push origin feature-xyz`
5. Open a Pull Request.

## License

Numan is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for details.