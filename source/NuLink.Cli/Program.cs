using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace NuLink.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var rootCommand = BuildCommandLine();
            return rootCommand.InvokeAsync(args).Result;
        }
        
        private static RootCommand BuildCommandLine()
        {
            var consumerOption = new Option(new[] { "--consumer", "-c" }, HelpText.ProjectOption, new Argument<string>() {
                Name = "file-path",
                Arity = ArgumentArity.ZeroOrOne
            });
            var packageOption = new Option(new[] { "--package", "-p" }, HelpText.PackageOption, new Argument<string>() {
                Name = "package-id",
                Arity = ArgumentArity.ZeroOrOne
            });
            var localProjectOption = new Option(new[] { "--local", "-l" }, HelpText.LocalProjectOption, new Argument<string>() {
                Name = "project-path",
                Arity = ArgumentArity.ExactlyOne
            });
            var dryRunOption = new Option(new[] { "--dry-run", "-d" }, HelpText.DryRunOption, new Argument<bool>() {
                Name = "on/off",
                Arity = ArgumentArity.ZeroOrOne
            });
            var quietOption = new Option(new[] { "--quiet", "-q" }, HelpText.QuietOption, new Argument<bool>() {
                Name = "on/off",
                Arity = ArgumentArity.ZeroOrOne
            });
            var rootDirOption = new Option(new[] { "--root-dir", "-r" }, HelpText.RootDirOption, new Argument<string>() {
                Name = "root-directory",
                Arity = ArgumentArity.ExactlyOne
            });

            return new RootCommand() {
                new Command("status", HelpText.StatusCommand, handler: HandleStatus()) {
                    consumerOption,
                    packageOption,
                    quietOption
                },
                new Command("link", HelpText.LinkCommand, handler: HandleLink()) {
                    consumerOption,
					rootDirOption,
                    packageOption,
                    localProjectOption,
                    dryRunOption
                },
                new Command("unlink", HelpText.UnlinkCommand, handler: HandleUnlink()) {
                    consumerOption,
					rootDirOption,
                    packageOption,
                    dryRunOption
                }
            };
        }

        private static ICommandHandler HandleStatus() => 
            CommandHandler.Create<string, string, bool>((consumer, package, quiet) => {
                var options = new NuLinkCommandOptions(
                    ValidateConsumerProject(consumer), 
                    package,
                    bareUI: quiet);
                return ExecuteCommand(
                    "status", 
                    options, 
                    options.ConsumerProjectPath != null);
            });

        private static ICommandHandler HandleLink() => 
            CommandHandler.Create<string, string, string, string, bool>((consumer, rootDir, package, local, dryRun) => {
                var options = new NuLinkCommandOptions(
                    ValidateConsumerProject(consumer), 
					rootDir,
                    package,
                    localProjectPath: local,
                    dryRun: dryRun);
                return ExecuteCommand(
                    "link", 
                    options, 
                    options.ConsumerProjectPath != null && !string.IsNullOrEmpty(rootDir)
                        ? ValidateRootDirectory(rootDir)
                        : ValidateTargetProject(options.LocalProjectPath) && options.PackageId != null);
            });

        private static ICommandHandler HandleUnlink() => 
            CommandHandler.Create<string, string, string, bool>((consumer, rootDir, package, dryRun) => {
                var options = new NuLinkCommandOptions(
                    ValidateConsumerProject(consumer), 
					rootDir,
                    package,
                    dryRun: dryRun);
                return ExecuteCommand(
                    "unlink", 
                    options, 
                    options.ConsumerProjectPath != null &&
                    (ValidateRootDirectory(rootDir) || options.PackageId != null));
            });
        
        private static int ExecuteCommand(
            string commandName, 
            NuLinkCommandOptions options,
            bool isValid)
        {
            if (!isValid)
            {
                return 1;
            }
            
            try
            {
                var command = CreateCommand(commandName, options);
                command.Execute(options);
                return 0;
            }
            catch (Exception e)
            {
                FullUI.FatalError(() => $"Fatal error: {e.Message}\nException: {e}");
                return 100;
            }
        }

        private static string ValidateConsumerProject(string filePath)
        {
            if (filePath != null)
            {
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                
                FullUI.FatalError(() => $"Error: File does not exist: {filePath}");
                return null;
            }
            
            filePath = Directory
                .GetFiles(Directory.GetCurrentDirectory(), "*.*")
                .FirstOrDefault(path => 
                    path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || 
                    path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase));

            if (filePath == null)
            {
                FullUI.FatalError(() => $"Error: No .sln/.csproj file found in current directory, and {"--consumer"} was not specified");
                return null;
            }

            return filePath;
        }

        private static bool ValidateTargetProject(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }
                
            FullUI.FatalError(() => $"Error: File does not exist: {filePath}");
            return false;
        }

        private static bool ValidateRootDirectory(string rootDir)
        {
            if (Directory.Exists(rootDir))
            {
                return true;
            }

            FullUI.FatalError(() => $"Error: Directory does not exist: {rootDir}");
            return false;
        }

        private static INuLinkCommand CreateCommand(string name, NuLinkCommandOptions options)
        {
            var ui = (options.BareUI ? new BareUI() : new FullUI() as IUserInterface);
            
            switch (name)
            {
                case "status":
                    return new StatusCommand(ui);
                case "link":
                    return new LinkCommand(ui);
                case "unlink":
                    return new UnlinkCommand(ui);
                default:
                    throw new Exception($"Command not supported: {name}.");
            }
        }

        private static class HelpText
        {
            public const string StatusCommand = 
                "Check status of packages in consumer project/solution";
            public const string LinkCommand = 
                "Link package to a directory in local file system, machine-wide";
            public const string UnlinkCommand = 
                "Unlink package from local file system, machine-wide";
            public const string ProjectOption = 
                "Path to consumer .csproj or .sln (default: first .csproj or .sln in current directory)";
            public const string PackageOption = 
                "Package id (default: all packages in consumer project/solution)";
            public const string LocalProjectOption = 
                "Full path to package .csproj in local file system";
            public const string DryRunOption = 
                "If specified, list intended actions without executing them";
            public const string QuietOption = 
                "If specified, suppresses all output except data (useful for scripting)";
            public const string RootDirOption = 
                "If specified, executes command on all packages in project/solution";
        }
    }
}