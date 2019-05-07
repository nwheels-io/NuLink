# NuLink

The missing NuGet LINK feature. This is a development-time feature, which allows consuming NuGet packages right from their source code on local machine. [Why?](#Why)

## How it works

NuLink creates symbolic links to resolve binaries of certain NuGet packages directly from local file system:

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

In this way, every time the package project is compiled, latest binaries are automatically used on the consumer side.

## Installation and usage

### Requirements

- Linux, macOS, or Windows
- .NET Core SDK 2.1+ (not tested on 3.0)

### Installation

```
$ dotnet tool install -g NuLink --version 0.1.0-alpha1 --add-source https://ci.appveyor.com/nuget/nulink-3672eibylf8q 
```

After the installation, the tool can be run from terminal with `nulink` command.

### Check status of packages

To check status of packages referenced by project or solution, run from project/solution directory:
```
$ nulink status
```

### Link package to local folder

First, make sure you have the sources of the package, and you did `dotnet restore` on the package project. Then in terminal, go to consumer project or solution directory and run:
```
$ nulink link -p My.Package -l /path/to/my/package/source/My.Package.csproj
```

### Un-Link package from local folder

Go to consumer project or solution directory, and run:
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

## Why

When a NuGet package has to be enhanced or fixed to fit the requirements of a consumer package or app, it often takes multiple iterations. During the iterations, changes in the provider package are tested on the consumer side, and sometimes consumer side is modified to make the ends meet. 

Here having a long loop (modify package, commit changes, publish new version, consume new version) is counter-productive. This is especially true when working on a project that includes multiple internal NuGet packages.

In the Node community, this problem is long solved with symbolic links and the `npm link` command, together with higher-order tools like `lerna` and `yarn workspaces`.



