#if false

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Buildalyzer;
using Buildalyzer.Environment;

namespace NuLink.Cli
{
    public class PublishAssembliesResolver
    {


        public IEnumerable<Result> Resolve(IEnumerable<ProjectAnalyzer> projects)
        {
            var envOptions = new EnvironmentOptions {
                Preference = EnvironmentPreference.Core
            };
            var environment = projects.First().EnvironmentFactory.GetBuildEnvironment(envOptions);
            var projectList = projects.ToList();

            return MapDependencyAssemblyLocations(projectList, environment);
        }
        
        private IEnumerable<Result> MapDependencyAssemblyLocations(
            IReadOnlyList<ProjectAnalyzer> projects, 
            BuildEnvironment environment)
        {
            var sdksFolderPath = environment.EnvironmentVariables["SdkPath"];
            var tempProjectFilePath = Path.Combine(Path.GetTempPath(), $"nulink_{Guid.NewGuid().ToString("N")}.proj");
            var tempProjectXml = GenerateResolvePublishAssembliesProject(projects, sdksFolderPath);

            using (var tempFile = File.Create(tempProjectFilePath))
            {
                tempProjectXml.Save(tempFile);
                tempFile.Flush();
            }

            try
            {
                var programRunner = new ExternalProgramRunner();
                programRunner.Run(
                    out IEnumerable<string> output, 
                    "dotnet", new[] { "msbuild", tempProjectFilePath, "/nologo" });

                return ParseAssemblyDirectoryMap(output);
            }
            finally
            {
                File.Delete(tempProjectFilePath);
            }
        }

        private XElement GenerateResolvePublishAssembliesProject(
            IReadOnlyList<ProjectAnalyzer> projects,
            string sdksDirectory)
        {
            XNamespace ns = @"http://schemas.microsoft.com/developer/msbuild/2003";

            var projectFilesItemGroupElement = new XElement(
                ns + "ItemGroup", 
                projects.Select(project => CreateProjectFileItemElement(ns, project)));

            var projectElement = new XElement(ns + "Project",
                new XElement(ns + "UsingTask",
                    new XAttribute("TaskName", "ResolvePublishAssemblies"),
                    new XAttribute(
                        "AssemblyFile",  //TODO: determine this path programmatically
                        Path.Combine(sdksDirectory, "tools", "netcoreapp2.0", "Microsoft.NET.Build.Tasks.dll"))),
                projectFilesItemGroupElement,
                new XElement(ns + "Target",
                    new XAttribute("Name", "Build"),
                    new XElement(ns + "ResolvePublishAssemblies",
                        new XAttribute("ProjectPath", "@(ProjectFiles)"),
                        new XAttribute("AssetsFilePath", "%(AssetsJson)"),
                        new XAttribute("TargetFramework", "%(TargetFramework)"),
                        new XElement(ns + "Output",
                            new XAttribute("TaskParameter", "AssembliesToPublish"),
                            new XAttribute("ItemName", "ResolvedAssembliesToPublish"))),
                    new XElement(ns + "Message",
                        new XAttribute("Text", "%(DestinationSubPath)=@(ResolvedAssembliesToPublish->Distinct())"),
                        new XAttribute("Importance", "high"))));

            return projectElement;
        }

        private XElement CreateProjectFileItemElement(XNamespace ns, ProjectAnalyzer project)
        {
            var projectFilePath = project.ProjectFile.Path;
            
            var itemElement = new XElement(ns + "ProjectFiles",
                new XAttribute("Include", projectFilePath),
                new XElement(ns + "TargetFramework", project.ProjectFile.TargetFrameworks.First()),
                new XElement(ns + "AssetsJson", Path.Combine(
                    Path.GetDirectoryName(projectFilePath), 
                    "obj", 
                    "project.assets.json")));

            return itemElement;
        }

        private IEnumerable<Result> ParseAssemblyDirectoryMap(IEnumerable<string> nameValuePairLines)
        {
            var map = new Dictionary<string, string>();

            foreach (var line in nameValuePairLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var nameValueParts = line.Trim().Split('=');

                if (nameValueParts.Length == 2 && !string.IsNullOrEmpty(nameValueParts[0]) && !string.IsNullOrEmpty(nameValueParts[1]))
                {
                    //TODO: skip assemblies that come from runtimes/<RID>/... (RID = Runtime IDentifier, e.g. win7-x64, debian-x64)
                    //      since such assemblies are listed multiple times (per each RID), 
                    //      and we don't filter by any specific RID, it is unreliable anyway
                    //      + those assemblies don't seem to ever arrive to AssemblyLoadContext.Resolving

                    AddAssemblyDirectoryMapEntry(map, assemblyPart: nameValueParts[0], directoryPart: nameValueParts[1]);
                }
                else
                {
                    Console.WriteLine($"Warning: Assembly directory pair could not be parsed: {line}");
                }
            }

            return map.Select(kvp => new Result(kvp.Key, kvp.Value));
        }
        
        private void AddAssemblyDirectoryMapEntry(Dictionary<string, string> map, string assemblyPart, string directoryPart)
        {
            var fileName = Path.GetFileName(assemblyPart);
            var fileExtension = Path.GetExtension(fileName).ToLower();
            string assemblyName;

            if (fileExtension == ".exe" || fileExtension == ".dll" || fileExtension == ".so")
            {
                //TODO: inspect assembly and extract its correct name; will this be a performance problem?
                //      for now assume that assembly name equals to file name (this default covers 99% of the cases)
                assemblyName = fileName.Substring(0, fileName.Length - fileExtension.Length);
            }
            else
            {
                assemblyName = fileName;
            }

            if (!map.ContainsKey(assemblyName))
            {
                var directorySubParts = directoryPart.Split(';');
                if (directorySubParts.Length > 1)
                {
                    directorySubParts = directorySubParts.OrderByDescending(s => s).ToArray();
                }
                map.Add(assemblyName, directorySubParts[0]);
            }
        }

        public class Result
        {
            public Result(string assemblyFile, string fullPath)
            {
                AssemblyFile = assemblyFile;
                FullPath = fullPath;
            }

            public string AssemblyFile { get; }
            public string FullPath { get; }
        }
    }
}

#endif
#if false

using NWheels.Cli.Publish;
using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using NWheels.Extensions;
using System;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NWheels.Cli.Run
{
    public class RunCommand : CommandBase
    {
        private string _microserviceFolderPath;
        private string _microserviceFilePath;
        private string _environmentFilePath;
        private bool _noPublish;
        private string _projectConfigurationName;
        private MicroserviceFolderType _microserviceFolderType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RunCommand() : base(
            "run",
            "runs microservice from publish folder (optionally performs publish first)")
        {
            _microserviceFolderPath = Directory.GetCurrentDirectory();
            _projectConfigurationName = "Debug";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineOption("n|no-publish", 
                ref _noPublish, 
                help: "run without publish: load modules from where they were compiled");

            syntax.DefineOption("m|microservice-xml",
                ref _microserviceFilePath, requireValue: true,
                help: "path to environment XML file to use");

            syntax.DefineOption("e|environment-xml",
                ref _environmentFilePath, requireValue: true,
                help: "path to environment XML file to use");

            syntax.DefineOption("p|project-config", 
                ref _projectConfigurationName, requireValue: true, 
                help: "project configuration name, when used with source folder (default: Debug)");

            syntax.DefineParameter("microservice-folder", 
                ref _microserviceFolderPath, 
                "microservice publish or source folder");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ValidateArguments(ArgumentSyntax arguments)
        {
            if (string.IsNullOrEmpty(_microserviceFolderPath))
            {
                arguments.ReportError("microservice folder must be specified");
            }

            _microserviceFolderPath = Path.GetFullPath(_microserviceFolderPath);

            if (!Directory.Exists(_microserviceFolderPath))
            {
                arguments.ReportError($"folder does not exist: {_microserviceFolderPath}");
            }

            if (string.IsNullOrEmpty(_microserviceFilePath))
            {
                _microserviceFilePath = Path.Combine(_microserviceFolderPath, BootConfiguration.MicroserviceConfigFileName);
            }

            if (!File.Exists(_microserviceFilePath))
            {
                arguments.ReportError($"file does not exist: {_microserviceFilePath}");
            }

            if (string.IsNullOrEmpty(_environmentFilePath))
            {
                _environmentFilePath = Path.Combine(_microserviceFolderPath, BootConfiguration.EnvironmentConfigFileName);
            }

            if (!File.Exists(_environmentFilePath))
            {
                arguments.ReportError($"file does not exist: {_environmentFilePath}");
            }

            if (!DetermineFolderType(out _microserviceFolderType))
            {
                arguments.ReportError("specified microservice folder is neither source nor publish");
            }

            if (_noPublish && _microserviceFolderType != MicroserviceFolderType.Source)
            {
                arguments.ReportError("--no-publish cannot be used with a published microservice");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Execute()
        {
            if (_microserviceFolderType == MicroserviceFolderType.Source && !_noPublish)
            {
                PublishMicroservice();
            }

            RunMicroservice();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool DetermineFolderType(out MicroserviceFolderType type)
        {
            if (Directory.GetFiles(_microserviceFolderPath, "System.*.dll", SearchOption.TopDirectoryOnly).Any() &&
                File.Exists(Path.Combine(_microserviceFolderPath, BootConfiguration.MicroserviceConfigFileName)) &&
                File.Exists(Path.Combine(_microserviceFolderPath, BootConfiguration.EnvironmentConfigFileName)))
            {
                type = MicroserviceFolderType.Publish;
                return true;
            }

            if (Directory.GetFiles(_microserviceFolderPath, "*.*proj", SearchOption.TopDirectoryOnly).Length == 1)
            {
                type = MicroserviceFolderType.Source;
                return true;
            }

            // folder type cannot be determined
            type = MicroserviceFolderType.Unknown;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PublishMicroservice()
        {
            var publish = new PublishCommand();
            var arguments = ArgumentSyntax.Parse(
                new[] { publish.Name, _microserviceFolderPath, "--no-cli", "--project-config", _projectConfigurationName },
                syntax => publish.BindToCommandLine(syntax)
            );
            publish.ValidateArguments(arguments);
            publish.Execute();

            _microserviceFolderPath = publish.PublishFolderPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunMicroservice()
        {
            try
            {
                LogImportant($"run > {_microserviceFolderPath}");

                var bootConfig = BootConfiguration.LoadFromFiles(_microserviceFilePath, _environmentFilePath);
                bootConfig.ConfigsDirectory = _microserviceFolderPath;

                if (_noPublish)
                {
                    MapAssemblyLocationsFromSources(bootConfig);
                }

                using (var host = new MicroserviceHost(bootConfig, new MicroserviceHostLoggerMock()))
                {
                    host.Configure();
                    host.LoadAndActivate();

                    LogSuccess("Microservice is up.");
                    LogSuccess("Press ENTER to go down.");

                    Console.ReadLine();

                    host.DeactivateAndUnload();
                }
            }
            catch (Exception ex)
            {
                ReportFatalError(ex);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void MapAssemblyLocationsFromSources(BootConfiguration bootConfig)
        {
            bootConfig.AssemblyMap = new AssemblyLocationMap();

            var solutionFolderPath = Directory.GetParent(bootConfig.ConfigsDirectory).FullName;
            var allProjectNames = Enumerable.Empty<string>()
                .Append(bootConfig.MicroserviceConfig.InjectionAdapter.Assembly)
                .Concat(bootConfig.MicroserviceConfig.FrameworkModules.Select(m => m.Assembly))
                .Concat(bootConfig.MicroserviceConfig.ApplicationModules.Select(m => m.Assembly));

            List<string> allProjectFilePaths = new List<string>();
            Dictionary<string, string> projectOutputAssemblyLocations = new Dictionary<string, string>();
            Dictionary<string, string> projectTargetFrameworkByFilePath = new Dictionary<string, string>();

            foreach (var projectName in allProjectNames)
            {
                if (TryFindProjectBinaryFolder(
                    projectName, 
                    solutionFolderPath, 
                    out string projectFilePath,
                    out string binaryFolderPath,
                    out string outputAssemblyFilePath,
                    out string projectTargetFramework))
                {
                    allProjectFilePaths.Add(projectFilePath);
                    projectTargetFrameworkByFilePath[projectFilePath] = projectTargetFramework;

                    bootConfig.AssemblyMap.AddDirectory(binaryFolderPath);

                    if (!string.IsNullOrEmpty(outputAssemblyFilePath))
                    {
                        projectOutputAssemblyLocations[projectName] = outputAssemblyFilePath;
                    }
                }
            }

            var dependencyAssemblyLocations = MapDependencyAssemblyLocations(allProjectFilePaths, projectTargetFrameworkByFilePath);
            bootConfig.AssemblyMap.AddLocations(dependencyAssemblyLocations);
            bootConfig.AssemblyMap.AddLocations(projectOutputAssemblyLocations);
            bootConfig.AssemblyMap.AddDirectory(AppContext.BaseDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryFindProjectBinaryFolder(
            string projectName, 
            string solutionFolderPath, 
            out string projectFilePath, 
            out string binaryFolderPath,
            out string assemblyFilePath,
            out string targetFramework)
        {
            projectFilePath = null;
            binaryFolderPath = null;
            assemblyFilePath = null;
            targetFramework = null;

            var projectFolderPath = Path.Combine(solutionFolderPath, projectName);
            if (!Directory.Exists(projectFolderPath))
            {
                return false;
            }

            projectFilePath = Directory.GetFiles(projectFolderPath, "*.*proj", SearchOption.TopDirectoryOnly).SingleOrDefault();
            if (projectFilePath == null)
            {
                return false;
            }

            var projectElement = XElement.Parse(File.ReadAllText(projectFilePath));
            var sdkAttribute = projectElement.Attribute("Sdk");
            if (projectElement.Name != "Project" || sdkAttribute == null || sdkAttribute.Value != "Microsoft.NET.Sdk")
            {
                return false;
            }

            var targetFrameworkElement = projectElement.XPathSelectElement("PropertyGroup/TargetFramework");
            var outputPathElement = projectElement.XPathSelectElement("PropertyGroup/OutputPath");

            var slash = Path.DirectorySeparatorChar;
            targetFramework = (targetFrameworkElement?.Value.DefaultIfNullOrEmpty(null) ?? "netstandard1.6");
            var outputPath = (
                outputPathElement?.Value.DefaultIfNullOrEmpty(null) ?? 
                $"bin{slash}{_projectConfigurationName}{slash}{targetFramework}");

            binaryFolderPath = Path.Combine(projectFolderPath, outputPath);
            if (!Directory.Exists(binaryFolderPath))
            {
                return false;
            }

            assemblyFilePath = Path.Combine(binaryFolderPath, projectName + ".dll");
            if (!File.Exists(assemblyFilePath))
            {
                assemblyFilePath = null;
            }

            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dictionary<string, string> MapDependencyAssemblyLocations(
            IEnumerable<string> projectFilePaths, 
            IReadOnlyDictionary<string, string> projectTargetFrameworkByFilePath)
        {
            var sdkDirectory = FindDotNetSdkBasePath();
            var sdkToolsDirectory = Path.Combine(sdkDirectory, "Sdks", "Microsoft.NET.Sdk");
            var tempProjectFilePath = Path.Combine(Path.GetTempPath(), $"nwheels_cli_{Guid.NewGuid().ToString("N")}.proj");
            var tempProjectXml = GenerateResolvePublishAssembliesProject(projectFilePaths, projectTargetFrameworkByFilePath, sdkToolsDirectory);

            using (var tempFile = File.Create(tempProjectFilePath))
            {
                tempProjectXml.Save(tempFile);
                tempFile.Flush();
            }

            try
            {
                ExecuteProgram(
                    out IEnumerable<string> output, 
                    "dotnet", new[] { "msbuild", tempProjectFilePath, "/nologo" });

                var parsedMap = ParseAssemblyDirectoryMap(output);
                return parsedMap;
            }
            finally
            {
                File.Delete(tempProjectFilePath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string FindDotNetSdkBasePath()
        {
            var basePathPhrase = "base path:";

            ExecuteProgram(out IEnumerable<string> output, "dotnet", new[] { "--info" });
            var basePathLine = output
                .Select(s => s.Trim())
                .FirstOrDefault(s => s.ToLower().StartsWith(basePathPhrase));

            if (basePathLine != null)
            {
                var basePath = basePathLine.Substring(basePathPhrase.Length).Trim();
                LogDebug($".NET SDK base path = {basePath}");
                return basePath;
            }

            throw new Exception("SDK installation directory cannot be determined from 'dotnet --info' output.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private XElement GenerateResolvePublishAssembliesProject(
            IEnumerable<string> projectFilePaths,
            IReadOnlyDictionary<string, string> projectTargetFrameworkByFilePath,
            string sdkDirectory)
        {
            XNamespace ns = @"http://schemas.microsoft.com/developer/msbuild/2003";

            var projectFilesItemGroupElement = new XElement(ns + "ItemGroup", 
                projectFilePaths.Select(item => CreateProjectFileItemElement(ns, item, projectTargetFrameworkByFilePath[item])));

            var projectElement = new XElement(ns + "Project",
                new XElement(ns + "UsingTask",
                    new XAttribute("TaskName", "ResolvePublishAssemblies"),
                    new XAttribute(
                        "AssemblyFile",  //TODO: determine this path programmatically
                        Path.Combine(sdkDirectory, "tools", "netcoreapp1.0", "Microsoft.NET.Build.Tasks.dll"))),
                projectFilesItemGroupElement,
                new XElement(ns + "Target",
                    new XAttribute("Name", "Build"),
                    new XElement(ns + "ResolvePublishAssemblies",
                        new XAttribute("ProjectPath", "@(ProjectFiles)"),
                        new XAttribute("AssetsFilePath", "%(AssetsJson)"),
                        new XAttribute("TargetFramework", "%(TargetFramework)"),
                        new XElement(ns + "Output",
                            new XAttribute("TaskParameter", "AssembliesToPublish"),
                            new XAttribute("ItemName", "ResolvedAssembliesToPublish"))),
                    new XElement(ns + "Message",
                        new XAttribute("Text", "%(DestinationSubPath)=@(ResolvedAssembliesToPublish->Distinct())"),
                        new XAttribute("Importance", "high"))));

            return projectElement;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private XElement CreateProjectFileItemElement(XNamespace ns, string projectFilePath, string projectTargetFramework)
        {
            var itemElement = new XElement(ns + "ProjectFiles",
                new XAttribute("Include", projectFilePath),
                new XElement(ns + "TargetFramework", projectTargetFramework),
                new XElement(ns + "AssetsJson", Path.Combine(Path.GetDirectoryName(projectFilePath), "obj", "project.assets.json")));

            return itemElement;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dictionary<string, string> ParseAssemblyDirectoryMap(IEnumerable<string> nameValuePairLines)
        {
            var map = new Dictionary<string, string>();

            foreach (var line in nameValuePairLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var nameValueParts = line.Trim().Split('=');

                if (nameValueParts.Length == 2 && !string.IsNullOrEmpty(nameValueParts[0]) && !string.IsNullOrEmpty(nameValueParts[1]))
                {
                    //TODO: skip assemblies that come from runtimes/<RID>/... (RID = Runtime IDentifier, e.g. win7-x64, debian-x64)
                    //      since such assemblies are listed multiple times (per each RID), 
                    //      and we don't filter by any specific RID, it is unreliable anyway
                    //      + those assemblies don't seem to ever arrive to AssemblyLoadContext.Resolving

                    AddAssemblyDirectoryMapEntry(map, assemblyPart: nameValueParts[0], directoryPart: nameValueParts[1]);
                }
                else
                {
                    LogWarning($"Assembly directory pair could not be parsed: {line}");
                }
            }

            return map;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddAssemblyDirectoryMapEntry(Dictionary<string, string> map, string assemblyPart, string directoryPart)
        {
            var fileName = Path.GetFileName(assemblyPart);
            var fileExtension = Path.GetExtension(fileName).ToLower();
            string assemblyName;

            if (fileExtension == ".exe" || fileExtension == ".dll" || fileExtension == ".so")
            {
                //TODO: inspect assembly and extract its correct name; will this be a performance problem?
                //      for now assume that assembly name equals to file name (this default covers 99% of the cases)
                assemblyName = fileName.Substring(0, fileName.Length - fileExtension.Length);
            }
            else
            {
                assemblyName = fileName;
            }

            if (!map.ContainsKey(assemblyName))
            {
                var directorySubParts = directoryPart.Split(';');
                if (directorySubParts.Length > 1)
                {
                    directorySubParts = directorySubParts.OrderByDescending(s => s).ToArray();
                }
                map.Add(assemblyName, directorySubParts[0]);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private enum MicroserviceFolderType
        {
            Unknown,
            Source,
            Publish
        }
    }
}

#endif