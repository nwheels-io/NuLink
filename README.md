# NuLink

The missing LINK feature of NuGet. This is a development-time feature, which allows consuming NuGet packages right from their source code on local machine. [Why?](#Why)

[![Build status](https://ci.appveyor.com/api/projects/status/1fn8jkqoyrum4aiq?svg=true)](https://ci.appveyor.com/project/felix-b/nulink)

See also: [Installation and usage](#installation-and-usage) | [Limitations](#limitations) | [Troubleshooting](#troubleshooting) | [Contributing](#contributing)

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

In the above example, every time My.Package.csproj is compiled, the latest binaries from its `bin/Debug` are seamlessly used by package consumers. Since the binaries are mapped (through .pdb) to source code in the local working directory, code navigation and debugging on consumer side works seamlessly as well.

## Current status

The project is in early alpha. It should work for .NET Core and NETStandard packages and projects. Other combinations (OS/SDK/project system) weren't tested. See [Limitations](#Limitations) for details.

Please report bugs and suggestions in the repo [Issues](https://github.com/felix-b/NuLink/issues). If something goes wrong, see recovery steps in [Troubleshooting](#Troubleshooting).

Contributions are welcome :-) Read [Contributing](Contributing).

## Installation and usage

### Requirements

- Linux, macOS, or Windows
- .NET Core SDK 2.1+ (not tested on 3.0)

Although the tool itself requires .NET Core, it should support .NET Framework projects and packages as well (not tested).

### Installation

```
$ dotnet tool install -g NuLink --version 0.1.0-alpha1 --add-source https://ci.appveyor.com/nuget/nulink-3672eibylf8q 
```

After the installation, the tool can be run from terminal with `nulink` command.

To update to a newer version of the tool, run:
```
$ dotnet tool update -g NuLink --add-source https://ci.appveyor.com/nuget/nulink-3672eibylf8q 
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

### Get help on command

To get help on a specific command, e.g. `link`:

```
$ nulink link --help
```

### List existing commands

```
$ nulink --help
```

[Back to top](#NuLink)

## Why

When a NuGet package has to be enhanced or fixed to fit the requirements of a consumer package or app, it often takes multiple iterations. During the iterations, changes in the provider package are tested on the consumer side, and sometimes consumer side is modified to make the ends meet. 

Here having a long loop (modify package, commit changes, publish new version, consume new version) is counter-productive. This is especially true when working on a project that includes multiple internal NuGet packages.

In the Node community, this problem is long solved with symbolic links and the [npm link](https://docs.npmjs.com/cli/link.html) command, together with higher-order tools like [lerna](https://lerna.js.org/) and [yarn workspaces](https://yarnpkg.com/lang/en/docs/workspaces/).

[Back to top](#NuLink)

## Contributing

All tickets and conversations are managed in the Issues section.

To contribute, please fork and submit a PR.

The CI build is currently on AppVeyor (see badge in the top).

Tests and coverage reports/checks will be added later.

More: TBD

[Back to top](#NuLink)

## Limitations

Supporting the entire variety of NuGet setups and workflows is hardly feasible. (Probably that's why NuGet still lacks the linking feature). We opt for supporting subset of scenarios in favor of never having a tool.

The following limitations apply:

Limitation|Roadmap
---|---
Consumer projects must be .NET Core or NETStandard (.NET Framework projects aren't supported)|Support of .NET Framework projects will be added
Not tested on .NET Core 3.0|Will be tested
Consumer projects must be C# (.csproj)|Support of other language projects will be added.
Symbolic link is always created to `bin/Debug` regardless of existing/desired configuration|Will be enhanced
Packages must be linked one by one|Linking multiple packages at once will be added
Package `lib` folder must be result of compiling a single project (e.g. automatic packaging of project on build). Arbitrary contents of `lib` that can be achieved with manually authored `.nuspec` is not supported.|Won't fix
The effect of symbolic link is machine-wide (not per consuming project/solution)|Won't fix


[Back to top](#NuLink)

## Troubleshooting

### Checking current situation

To check the current situation of symbolic links in your NuGet packages, run one of the following commands, depending on your OS:

macOS and Linux:
```
$ cd ~
$ find . -type l -ls
```

Windows:
```
> cd %UserProfile%
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
