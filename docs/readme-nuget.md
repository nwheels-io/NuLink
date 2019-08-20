NuLink allows consuming NuGet packages from source code on local machine. This creates seamless environment where packages can be developed and tested as if their code was part of the main project. [Learn more on GitHub](https://github.com/nwheels-io/NuLink/blob/master/README.md)

[![Build status](https://ci.appveyor.com/api/projects/status/1fn8jkqoyrum4aiq/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nulink) [![Similar solutions](https://img.shields.io/badge/inspired%20by-npm%20link-blue.svg)](https://docs.npmjs.com/cli/link)

## Getting started

### Supported types of projects

- .NET Core and NETStandard projects and packages (_"SDK/PackageReference-style"_)
- **New in Beta 2**: .NET Framework projects and packages (_"packages.config-style"_)

### Prerequisites

- Linux, macOS, or Windows
- .NET Core SDK 2.1+ (not tested on 3.0 yet)

### Installing

```
$ dotnet tool install -g NuLink --version 0.1.0-beta2
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

NuLink creates symbolic links to consume binaries of selected packages directly from their compilation directories in the local file system. 

### For SDK/PackageReference-style projects (.NET Core or NETStandard)

```
Original                      Linked
--------------------          ----------------------
~ or %UserProfile%            working directory
|                             |
+- .nuget/                    +- My.Package/
   |                             | 
   +- packages/                  +- Source/
      |                             |
      +- my.package/                +- My.Package.csproj     
         |                          |  
         +- 1.0.5/                  +- bin/
            |                          |
            +- lib >---> SYMLINK >---> +- Debug/
               |                          |
               +-X- netstandard2.0/       +-V- netstandard2.0/
```

In this example, every time `My.Package.csproj` is compiled, the latest binaries from its `bin/Debug` are automatically used by all consumers. Since .pdb in `bin/Debug` maps the binaries to local sources, code navigation and debugging on consumer side work seamlessly with the latest changes in package code.

### For packages.config-style projects (.NET Framework)

```
Original                        Linked
--------------------            ----------------------
consumer working directory    
| 
+- Source\                      package working directory    
   |                            | 
   +- consumer-solution.sln     +- My.Package
   |                               |
   +- packages\                    +- Source\
      |                               |
      +- My.Package.1.0.5\            +- My.Package.csproj     
         |                            |  
         +- lib\                      +- bin\
            |                            |
            +- net45 >---> SYMLINK >---> +- Debug\
```

This example works mostly like the previous one, except that the link only affects a specific consumer solution. This is because in .NET Framework projects, packages are copied under a solution-level `packages` folder, whereas in the new SDK-style projects, .NET looks for packages in the user-level cache.

