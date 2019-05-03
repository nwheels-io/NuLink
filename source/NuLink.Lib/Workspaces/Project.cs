using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NuLink.Lib.Abstractions;
using NuLink.Lib.Common;
using NuLink.Lib.MsBuildFormat;
using Semver;

namespace NuLink.Lib.Workspaces
{
    public class Project
    {
        public const string BackupFileSuffix = ".nulink-backup";
        
        public Project(
            IImmutableEnvironment environment,
            ISourceControl sourceControl,
            Solution solution, 
            ProjectSlnFilePart slnFilePart)
        {
            this.Solution = solution;
            this.SlnFilePart = slnFilePart;
            this.FileInfo = solution.Sln.GetProjectFileInfo(slnFilePart);
            this.BackupFileInfo = FileInfo.WithFileExtension(BackupFileSuffix);
            this.Csproj = new CsprojFile(FileInfo, environment.LoadXml(FileInfo.FullName));
            this.VersionElement =
                ProjectSlnFilePart.TryFindProjectVersionElement(Xml) ??
                ProjectSlnFilePart.AddProjectVersionElement(Xml, "0.1.0-alpha1");
            this.Version = SemVersion.Parse(VersionElement.Value);
            this.DependencyProjects = new List<Dependency>();
            this.DependentProjects = new List<Dependency>();
            this.Repo = sourceControl.GetRepoOfDirectory(FileInfo.Directory);
            this.HasOwnChanges = DetectOwnChanges();
            this.HasUnpushedCommits = false;
        }

        public void PopulateDependencyProjects()
        {
            //TODO: implement this
        }
        
        public void PopulateDependentProjects(IEnumerable<Dependency> allDepsInSolution)
        {
            foreach (var dependency in allDepsInSolution.Where(dep => dep.Principal == this))
            {
                DependentProjects.Add(dependency);
            }
        }

        public Solution Solution { get; }
        public ProjectSlnFilePart SlnFilePart { get; }
        public string Name => SlnFilePart.Name;
        public XElement Xml => Csproj.Xml;
        public CsprojFile Csproj { get; }
        public SemVersion Version { get; }
        public FileInfo FileInfo { get; }
        public FileInfo BackupFileInfo { get; private set; }
        public ISourceRepository Repo { get; }
        public XElement VersionElement { get; }
        public bool HasOwnChanges { get; private set; }
        public bool HasDepenencyChanges { get; private set; }
        public bool HasUnpushedCommits { get; private set; }
        public string PendingCommitMessage { get; set; }
        public List<Dependency> DependencyProjects { get; }
        public List<Dependency> DependentProjects { get; }  
        
        private bool DetectOwnChanges()
        {
            //TODO: implement this
            return false;
        }
    }
}
