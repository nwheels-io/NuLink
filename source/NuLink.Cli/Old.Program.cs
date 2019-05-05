#if false

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using Autofac;
using NuLink.Lib;
using NuLink.Lib.Abstractions;

namespace NuLink.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("NuLink tool");
            var commandLine = BuildCommandLine();
            return commandLine.InvokeAsync(args).Result;
        }

        private static RootCommand BuildCommandLine()
        {
            var slnFileOption =  
                new Option(
                    new[] { "--sln-file", "-s" }, 
                    "Path so solution file. Default: .sln file in current directory", 
                    new Argument<FileInfo>() 
                {
                    Arity = ArgumentArity.ZeroOrOne,
                });

            var configFileOption =  
                new Option(
                    new[] { "--config-file", "-c" }, 
                    "Path to NuLink config file. Default: NuLink.config in solution directory", 
                    new Argument<FileInfo>() 
                {
                    Arity = ArgumentArity.ZeroOrOne,
                });

            Func<IArgumentArity, Option> createPackagesOption = (arity) => 
                new Option(
                    new [] { "--package-names", "-p" }, 
                    "Package name(s) separated by space" + 
                        (arity.MinimumNumberOfArguments == 0 
                        ? ". Default: all packages" 
                        : ""), 
                    new Argument<string[]>() 
                {
                    Arity = arity
                });
            
            return new RootCommand {
                new Command(
                    name: "init",
                    description: "Initialize NuLink configuration for solution",
                    handler: HandleInit()) 
                {
                    slnFileOption,
                    configFileOption
                },
                new Command(
                    name: "merge",
                    description: "Merge NuGet packages into solution",
                    handler: HandleMerge()) 
                {
                    slnFileOption,
                    configFileOption,
                    createPackagesOption(ArgumentArity.OneOrMore),
                },
                new Command(
                    name: "unmerge",
                    description: "Unmerge NuGet packages from solution",
                    handler: HandleUnmerge()) 
                {
                    slnFileOption,
                    configFileOption,
                    createPackagesOption(ArgumentArity.ZeroOrMore)
                },
                new Command(
                    name: "release",
                    description: "Release changes in NuGet packages currently merged into solution",
                    handler: HandleRelease()) 
                {
                    slnFileOption,
                    configFileOption,
                    createPackagesOption(ArgumentArity.ZeroOrMore)
                },
                new Command(
                    name: "status",
                    description: "List NuGet packages currently merged into solution",
                    handler: HandleStatus()) 
                {
                    slnFileOption,
                    configFileOption
                }
            };

        }

        private static ICommandHandler HandleInit() => 
            CommandHandler.Create<FileInfo, FileInfo>((slnFile, configFile) => {
                return ExecuteCommand(slnFile, configFile, false, null, "init");
            });

        private static ICommandHandler HandleMerge() => 
            CommandHandler.Create<FileInfo, FileInfo, object>((slnFile, configFile, packageNames) => {
                return ExecuteCommand(slnFile, configFile, true, packageNames, "merge");
            });

        private static ICommandHandler HandleUnmerge() => 
            CommandHandler.Create<FileInfo, FileInfo, object>((slnFile, configFile, packageNames) => {
                return ExecuteCommand(slnFile, configFile, true, packageNames, "unmerge");
            });

        private static ICommandHandler HandleRelease() => 
            CommandHandler.Create<FileInfo, FileInfo, object>((slnFile, configFile, packageNames) => {
                return ExecuteCommand(slnFile, configFile, true, packageNames, "release");
            });

        private static ICommandHandler HandleStatus() => 
            CommandHandler.Create<FileInfo, FileInfo>((slnFile, configFile) => {
                return ExecuteCommand(slnFile, configFile, true, null, "status");
            });

        private static int ExecuteCommand(
            FileInfo slnFile, 
            FileInfo configFile,
            bool configFileMustExist,
            object packageList, 
            string commandName)
        {
            slnFile = ValidateSolutionFile(slnFile);
            if (slnFile == null)
            {
                return 1;
            }

            configFile = ValidateConfigFile(slnFile, configFile, configFileMustExist);
            if (configFile == null)
            {
                return 1;
            }

            var packageNameArray = (packageList as string[]) ?? new string[0];
            var options = new CommandOptions(
                solution: slnFile,
                configuration: configFile,
                packageNames: packageNameArray,
                dryRun: false);
            
            try
            {
                var command = CreateCommand(options, commandName);
                command.Execute(options);
                return 0;
            }
            catch (Exception e)
            {
                ConsoleUI.FatalError(() => $"Fatal error: {e.Message}\nException: {e}");
                return 100;
            }
        }

        private static INuLinkCommand CreateCommand(CommandOptions options, string commandName)
        {
            var factory = new CommandFactory(options, builder => {
                builder.RegisterType<ConsoleUI>().As<IUserInterface>().SingleInstance();
            });

            return factory.CreateCommand(commandName);
        }

        private static FileInfo ValidateSolutionFile(FileInfo file)
        {
            if (file != null)
            {
                return ValidateExistingFile(file, "Solution");
            }
            
            var solutionFilePath = Directory
                .GetFiles(Directory.GetCurrentDirectory(), "*.sln")
                .FirstOrDefault();

            if (solutionFilePath == null)
            {
                Console.Error.WriteLine("No .sln file found in current directory, and --sln-file option was not specified");
                return null;
            }
            
            return new FileInfo(solutionFilePath);
        }

        private static FileInfo ValidateConfigFile(FileInfo slnFile, FileInfo configFile, bool mustExist)
        {
            if (configFile != null)
            {
                return 
                    mustExist 
                    ? configFile
                    : ValidateExistingFile(configFile, "Configuration");
            }

            var defaultFilePath = Path.Combine(slnFile.Directory.FullName, "NuLink.config");

            if (!mustExist || File.Exists(defaultFilePath))
            {
                return new FileInfo(defaultFilePath);
            }
            
            Console.Error.WriteLine("No NuLink.config file found in solution directory, and --config-file option was not specified");
            return null;
        }

        private static FileInfo ValidateExistingFile(FileInfo file, string fileType)
        {
            if (!File.Exists(file.FullName))
            {
                Console.Error.WriteLine($"{fileType} file does not exist: " + file.FullName);
                return null;
            }

            return file;
        }

        private static void LogCommandOptions(string commandName, CommandOptions options)
        {
            Console.WriteLine("--- command options ---");
            Console.WriteLine($"Command:       {commandName}");
            Console.WriteLine($"Solution:      {options.Solution.FullName}");
            Console.WriteLine($"Configuration: {options.Configuration.FullName}");
            Console.WriteLine($"Packages:      [{string.Join(";", options.PackageNames)}]");
            Console.WriteLine("--- end of command options ---");
        }
    }
}

#endif