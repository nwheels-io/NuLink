using System.Linq;
using System.Threading.Tasks;
using NuLink.Lib.Abstractions;
using NuLink.Lib.NuGetFormat;
using NuLink.Lib.Workspaces;

namespace NuLink.Lib.Commands
{
    public class InitCommand : INuLinkCommand
    {
        private readonly IUserInterface _ui;
        private readonly IImmutableEnvironment _environment;
        private readonly IEnvironmentEffect _effect;
        private readonly IWorkspaceLoader _loader;
        private readonly JsonConfigPersistor _configPersistor;
        private readonly NuGetMetadataLoader _nugetLoader;

        public InitCommand(
            IUserInterface ui,
            IImmutableEnvironment environment,
            IEnvironmentEffect effect,
            IWorkspaceLoader loader,
            JsonConfigPersistor configPersistor,
            NuGetMetadataLoader nugetLoader)
        {
            _ui = ui;
            _environment = environment;
            _effect = effect;
            _loader = loader;
            _configPersistor = configPersistor;
            _nugetLoader = nugetLoader;
        }

        public void Execute(CommandOptions options)
        {
            var workspace = _loader.Load(options);

            var packageReferences = workspace.Solution.Projects
                .SelectMany(p => p.Csproj.GetPackageReferences())
                .Distinct();

            var packageConfigs = packageReferences
                .Select(AnalyzePackage)
                .ToList();
            
            _ui.ReportMedium(() => $"Found {packageConfigs.Count} packages");
            
            var configFile = new ConfigFile {
                Packages = packageConfigs
            };
            
            _configPersistor.Save(configFile, options.Configuration.FullName);
            _ui.ReportImportant(() => $"Saved configuration to {options.Configuration.FullName}");

            ConfigFilePackage AnalyzePackage(PackageReference package)
            {
                var packageConfig = new ConfigFilePackage();
                
                var packageMeta = _ui.TrackAsyncVerbose(() => $"Downloading package metadata: {package.Id}", () => {
                    return _nugetLoader.DownloadMetadata(package);
                }).Result;

                packageConfig.Id = package.Id;
                packageConfig.SourceRepoType = packageMeta.SourceRepoType;
                packageConfig.SourceRepo = packageMeta.SourceRepoUrl;
                packageConfig.ProjectsToMerge.Add($"Source/{package.Id}.csproj");
                
                _ui.ReportMedium(() =>
                    $"Package {package.Id} -> {packageConfig.SourceRepoType} -> {packageConfig.SourceRepo}");

                return packageConfig;
            }
        }
    }
}
