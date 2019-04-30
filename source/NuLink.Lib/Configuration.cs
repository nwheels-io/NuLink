using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Newtonsoft.Json;
using NuLink.Lib.Abstractions;
using NuLink.Lib.Workspaces;

namespace NuLink.Lib
{
    public class Configuration
    {
        public Configuration()
        {
            Packages = new Dictionary<string, PackageConfiguration>();
        }

        public Configuration(ConfigFile file, IComponentContext container)
        {
            Packages = file.Packages.ToDictionary(
                p => p.Id,
                p => new PackageConfiguration(p, container)
            );
        }
        
        public IReadOnlyDictionary<string, PackageConfiguration> Packages { get; }
    }
    
    public class PackageConfiguration
    {
        public PackageConfiguration(ConfigFilePackage package, IComponentContext container)
        {
            Id = package.Id;
            SourceRepo = package.SourceRepo;
            SourceRepoType = package.SourceRepoType;
            ProjectsToMerge = package.ProjectsToMerge.ToArray();
            SourceControl = container.ResolveKeyed<ISourceControl>(package.SourceRepoType);
        }
        
        public string Id { get; }
        public string SourceRepo { get; }
        public string SourceRepoType { get; }
        public ISourceControl SourceControl { get; }
        public IReadOnlyList<string> ProjectsToMerge { get; }
    }
    
    public class ConfigFile
    {
        public List<ConfigFilePackage> Packages { get; set; } = new List<ConfigFilePackage>();
    }

    public class ConfigFilePackage
    {
        public ConfigFilePackage()
        {
        }

        public ConfigFilePackage(PackageMetadata metadata)
        {
            Id = metadata.Reference.Id;
            SourceRepo = metadata.SourceRepoUrl;
            SourceRepoType = metadata.SourceRepoType;
        }

        public string Id { get; set; }
        public string SourceRepo { get; set; }
        public string SourceRepoType { get; set; } = "git";
        public List<string> ProjectsToMerge { get; set; } = new List<string>(); 
    }

    public class JsonConfigPersistor
    {
        private readonly IComponentContext _container;
        private readonly IImmutableEnvironment _environment;
        private readonly IEnvironmentEffect _effect;

        public JsonConfigPersistor(IComponentContext container, IImmutableEnvironment environment, IEnvironmentEffect effect)
        {
            _container = container;
            _environment = environment;
            _effect = effect;
        }

        public Configuration Load(string path)
        {
            var json = _environment.LoadTextFile(path);
            var deserialized = JsonConvert.DeserializeObject<ConfigFile>(json);
            return new Configuration(deserialized, _container);
        }

        public void Save(ConfigFile config, string path)
        {
            var json = JsonConvert.SerializeObject(config);
            _effect.SaveTextFile(json, path);
        }
    }
}
