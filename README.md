# Numan - NuGet Package Manager

Numan is a CLI tool that simplifies managing local (for now) NuGet packages. It automates tracking, updating, and adding `.nupkg` files to your local NuGet sources, reducing the manual overhead of managing packages in local development environments.

## Installation

### Prerequisites

- [.NET SDK 9.0 or later](https://dotnet.microsoft.com/download/dotnet/9.0)

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
numan add --package <package-path> --source <source> --configuration <configuration>
```

- `--package` (optional): Path to the `.nupkg` file.
- `--source`: Name of the NuGet source.
- `--configuration`: Specify build configuration (default: Debug).

If no package path is specified, Numan automatically detects `.nupkg` files in `bin/Debug` or `bin/Release`.

### List Installed Packages

To list all packages in a NuGet source:

```sh
numan list --source <source>
```

### List NuGet Sources

To view all tracked local NuGet sources:

```sh
numan list-sources
```

### Update Packages

To check for and add new package versions:

```sh
numan update --source <source>
```

To update all new versions automatically without prompts:

```sh
numan update --all
```

To manually select which packages to update:

```sh
numan update --allow-selection
```

### Remove Packages

To remove or delete packages from the local NuGet source:

```sh
numan remove --source <source> --all-versions
```

- `--all-versions`: Deletes the entire package, including all versions.

### Show Configuration

To display the current Numan configuration:

```sh
numan config show
```

### Set Default Source

To change the default NuGet source (if multiple sources exist):

```sh
numan config set-default
```

## Configuration

Numan stores configuration in `~/.numan`. This file tracks monitored package folders and NuGet sources. You can manually edit it if needed.

## Contributing

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature-xyz`
3. Commit your changes: `git commit -m "Add feature xyz"`
4. Push to the branch: `git push origin feature-xyz`
5. Open a Pull Request.

## License

Numan is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for details.
