# NuLink

NuLink allows consuming NuGet packages directly from source code on local machine. This creates seamless environment where packages can be developed and tested as if their code was in the main project. [Why?](#Why-would-you-use-it)

[![Build status](https://ci.appveyor.com/api/projects/status/1fn8jkqoyrum4aiq/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nulink) [![Nuget](https://img.shields.io/nuget/vpre/NuLink.svg)](https://www.nuget.org/packages/NuLink//)

See also: [Usage instructions](#usage-instructions) | [Limitations & roadmap](#limitations-and-roadmap) | [Troubleshooting](#troubleshooting) | [Contributing](CONTRIBUTING.md) | [Acknowledgements](#Acknowledgements)

## Current status

The project is currently in beta. It already works for both .NET Core/NETStandard and .NET Framework packages and projects. Some combinations (OS, consumer style, package style) weren't yet tested. See [Limitations & roadmap](#limitations-and-roadmap) for details.

Please report bugs and suggestions in the repo [Issues](https://github.com/nwheels-io/NuLink/issues). If something goes wrong, see recovery steps in [Troubleshooting](#Troubleshooting).

Contributions are welcome :-) Read [CONTRIBUTING.md](CONTRIBUTING.md).

## Getting started

### Supported types of projects

- .NET Core and NETStandard projects and packages (_"SDK/PackageReference-style"_)
- **New in Beta 2**: .NET Framework projects and packages (_"packages.config-style"_)

### Prerequisites

- Linux, macOS, or Windows
- .NET Core SDK 2.1+ (not tested on 3.0 yet)

### Installing

```
$ dotnet tool install --global NuLink --version 0.1.0-beta2
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

See [Usage instructions](#Usage-instructions) for more info.

[Back to top](#NuLink)

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

[Back to top](#NuLink)

## Why would you use it

Say you've found a piece of code that's a perfect candidate to become a reusable package. So you create a class library project, configure it to be packed for NuGet, and move the code there. 

That's all great, but now when making changes in the package, how do you try them out in your main project? Publishing a new version to NuGet every time you want to test your new lines of code just doesn't cut it.

There has to be a seamless environment, which lets you develop packages as if their code was in your main project.

In Node community this problem is long solved with symlinks using **[npm link](https://docs.npmjs.com/cli/link.html)** command. On top of that tools like **[Lerna](https://lerna.js.org/)** support whole development workflows.

[Back to top](#NuLink)

## Limitations and roadmap

Supporting the full variety of NuGet setups and workflows is hardly feasible. NuLink will initially support the more straightforward workflows, as listed in the table below. Eventually, support for more scenarios can be added. 

Limitation|Roadmap
---|---
**RESOLVED! 0.1.0-beta2:** NuLink now supports both SDK/PackageReference-style projects (.NET Core/NETStandard) and Old/packasges.config-style projects (.NET Framework)|[#2 Add support for .NET Framework projects](https://github.com/nwheels-io/NuLink/issues/2)
Not tested on .NET Core 3.0|[#3 Test on .NET Core 3.0](https://github.com/nwheels-io/NuLink/issues/3)
Consumer projects must be C# (.csproj)|[#4 Support projects in more languages](https://github.com/nwheels-io/NuLink/issues/4)
The `--dry-run` option is not implemented|[#19 Implement dry run](https://github.com/nwheels-io/NuLink/issues/19)
Symbolic link is always created to `bin/Debug` of the package, regardless of existing/desired build configuration|[#5 Add ability to detect and select package configuration](https://github.com/nwheels-io/NuLink/issues/5)
A package that's being developed and wasn't yet pushed to any NuGet feed, cannot be linked. This is because packages root folder (`~/.nuget/packages/`) must contain an entry for the package.|[#12 Allow linking unpushed packages by first automatically restoring them from temporary local feed](https://github.com/nwheels-io/NuLink/issues/12).
Packages must be linked one by one|[#6 Add ability to link multiple referenced packages at once](https://github.com/nwheels-io/NuLink/issues/6)
Package `lib` folder must be result of compiling a single project (e.g. automatic packaging of project on build). Packages with arbitrary contents of `lib` achieved with manually authored `.nuspec` are not supported.|Complex to solve ([#7](https://github.com/nwheels-io/NuLink/issues/7)). Wait to see if there's enough demand
For SDK-style consumer projects, the effect of symbolic link is machine-wide. It is not per consuming project/solution|Probably won't fix ([#8](https://github.com/nwheels-io/NuLink/issues/8))

[Back to top](#NuLink)

## Usage instructions

### Install, update, uninstall

Install:
```
$ dotnet tool install -g NuLink --version 0.1.0-beta2
```

After the installation, the tool can be run from terminal with `nulink` command.

To update to a newer version of the tool, run:
```
$ dotnet tool update -g NuLink
```

To uninstall the tool:
```
$ dotnet tool uninstall -g NuLink
```

### Check status of packages

To check status of packages referenced by project or solution, run from project/solution directory:
```
$ nulink status
```

### Link package to local folder

First, make sure you have the sources of the package, and you did `dotnet restore` and `dotnet build` on the package project. Then in terminal, go to consumer project or solution directory and run:
```
$ nulink link -p My.Package -l /path/to/my/package/source/My.Package.csproj
```
In the above example, all consumers of **My.Package** will start using binaries from `/path/to/my/package/source/bin/Debug`.

### Un-Link package from local folder

To revert symbolic link on a package, go to consumer project or solution directory, and run:
```
$ nulink unlink -p My.Package
```

### Get help 

To list existing commands:

```
$ nulink --help
```

To get help on a specific command, e.g. `link`:

```
$ nulink link --help
```

To check version of the tool:

```
$ nulink --version
```

[Back to top](#NuLink)

## Troubleshooting

### Checking current situation

To check the current situation of symbolic links in your NuGet packages, run one of the following commands, depending on your OS:

macOS and Linux:
```
$ cd ~/.nuget/packages
$ find . -type l -ls
```

Windows:
```
> cd %UserProfile%\.nuget\packages
> dir /al /s | findstr "<SYMLINKD>"
```

### Manually removing symbolic links

Example. To manually remove a link for **My.Package** version **1.0.5**, do these steps:

- Go to package version folder, usually `~/.nuget/packages/My.Package/1.0.5` on macOS and Linux, or `%UserProfile%\.nuget\packages\My.Package\1.0.5` on Windows
  - To verify the exact location of packages root folder,  go to one of the consuming projects. In the `obj`   directory, find a file with extension `.nuget.g.props`. In that file, find `<NuGetPackageRoot>` element, specifying packages root folder. From that folder, descend into -> `My.Package` -> `1.0.5`. 
- List files in directory (`ls` or `dir`). Make sure you find:
  - `lib` directory symlink pointing to `bin/Debug` of package source folder
  - `nulink-backup.lib` directory. This is the original `lib` directory before creation of symlink.
- Remove the `lib` directory with `rm` (Linux/macOS) or `del` (Windows). Note that this only removes the symlink, not the actual `bin/Debug` folder.
- Rename the `nulink-backup.lib` directory back to `lib`.

[Back to top](#NuLink)

## Authors

- [Felix Berman](https://github.com/felix-b) - initial work

[Back to top](#NuLink)

## Acknowledgements

This tool was inspired by **[npm link](https://docs.npmjs.com/cli/link.html)**.

These awesome libraries were used:

- [System.CommandLine](https://github.com/dotnet/command-line-api) by .NET Foundation ([license and copyright](https://github.com/dotnet/command-line-api/blob/master/LICENSE.md))
- [Buildalyzer](https://www.nuget.org/packages/Buildalyzer/) by Dave Glick ([license and copyright](https://github.com/daveaglick/Buildalyzer/blob/master/LICENSE))
- [Murphy.SymbolicLink](https://www.nuget.org/packages/Murphy.SymbolicLink/) by Thomas Chust ([license and copyright](https://bitbucket.org/chust/symboliclink/src/default/LICENSE.txt))

[Back to top](#NuLink)
