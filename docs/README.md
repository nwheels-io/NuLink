Allows consuming NuGet packages directly from source code on local machine. This creates seamless environment where packages can be developed and tested as if their code was in the main project. [Why?](#Why-would-you-use-it)

[![Build status](https://ci.appveyor.com/api/projects/status/1fn8jkqoyrum4aiq/branch/master?svg=true)](https://ci.appveyor.com/project/felix-b/nulink) [![Nuget](https://img.shields.io/nuget/vpre/NuLink.svg)](https://www.nuget.org/packages/NuLink//)

See also: [Usage instructions](#usage-instructions) | [Limitations & roadmap](../README.md#limitations-and-roadmap) | [Troubleshooting](#troubleshooting) | [Contributing](../CONTRIBUTING.md) | [Acknowledgements](../README.md#Acknowledgements)

## Getting started

### Prerequisites

- Linux, macOS, or Windows
- .NET Core SDK 2.1+ (not tested on 3.0)

Although the tool runs on .NET Core, it is planned to support .NET Framework projects and packages as well.

### Installing

```
$ dotnet tool install -g NuLink --version 0.1.0-beta1
```

### Linking a package to local sources

Prior to linking, make sure these conditions are met: 

- package must be first restored from a NuGet feed, usually by restoring a consumer project/solution (this limitation will be removed in upcoming versions)
- package source project must be located on the local machine (obviously)
- either `dotnet restore` or `dotnet build` must be run at least once on the package source project

In terminal, go to directory of a project/solution that consumes the package, and run:
```
$ nulink link -p My.Package -l /path/to/my/package/source/My.Package.csproj
```

In this example, all consumers of **My.Package** will start using binaries from `/path/to/my/package/source/bin/Debug`.

See [Usage instructions](#Usage-instructions) for more info.

[Back to top](#NuLink)

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

[Back to top](#NuLink)

## Why would you use it

Say you've found a piece of code that's a perfect candidate to become a reusable package. So you create a class library project, configure it to be packed for NuGet, and move the code there. 

That's all great, but now when making changes in the package, how do you try them out in your main project? Publishing a new version to NuGet every time you want to test your new lines of code just doesn't cut it.

There has to be a seamless environment, which lets you develop packages as if their code was in your main project.

In Node community this problem is long solved with symlinks using **[npm link](https://docs.npmjs.com/cli/link.html)** command. On top of that tools like **[lerna](https://lerna.js.org/)** support whole development workflows.

[Back to top](#NuLink)


## Usage instructions

### Install, update, uninstall

Install:
```
$ dotnet tool install -g NuLink --version 0.1.0-beta1
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
