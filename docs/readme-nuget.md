NuLink allows consuming NuGet packages from source code on local machine. This creates seamless environment where packages can be developed and tested as if their code was part of the main project. [Learn more on GitHub](https://github.com/nwheels-io/NuLink/blob/master/README.md)

[![Build status](https://ci.appveyor.com/api/projects/status/1fn8jkqoyrum4aiq/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nulink) [![Similar solutions](https://img.shields.io/badge/inspired%20by-npm%20link-blue.svg)](https://docs.npmjs.com/cli/link)

## Getting started

### Prerequisites

- Linux, macOS, or Windows
- .NET Core SDK 2.1+ (not tested on 3.0)

This version of NuLink supports SDK-style projects that use `<PackageReference>`. Support of .NET Framework-style projects that use `packages.config` is expected in upcoming versions.

### Installing

```
$ dotnet tool install -g NuLink --version 0.1.0-alpha3
```

### Linking a package to local sources

Prior to linking, make sure these conditions are met: 

- package must be first restored from a NuGet feed (this limitation will be removed in upcoming versions)
- package source project must be located on the local machine
- either `dotnet restore` or `dotnet build` must be run at least once on the package project

In terminal, go to directory of project/solution that consumes the package, and run:

```
$ nulink link -p My.Package -l /path/to/my/package/source/My.Package.csproj
```

In this example, all consumers of **My.Package** will start using binaries from `/path/to/my/package/source/bin/Debug`.

See [Usage instructions](https://github.com/nwheels-io/NuLink/blob/master/README.md#Usage-instructions) for more info.

## How it works

NuLink creates symbolic links to resolve binaries of selected packages directly from local file system:

```
Original                      Redirect
--------------------          ----------------------
~ or %UserProfile%            working directory
|                             |
+- .nuget/                    +- My.Package/
   |                             | 
   +- packages/                  +- Source/
      |                             |
      +- My.Package/                +- My.Package.csproj     
         |                          |  
         +- 1.0.5/                  +- bin/
            |                          |
            +- lib >---> SYMLINK >---> +- Debug/
               |                          |
               +-X- netstandard2.0/       +-V- netstandard2.0/
```

In this example, every time `My.Package.csproj` is compiled, the latest binaries from its `bin/Debug` are automatically used by all consumers. Since the binaries are mapped (through .pdb) to local sources, code navigation and debugging on consumer side work seamlessly with latest changes in package code.
