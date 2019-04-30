using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NuLink.Lib.Abstractions;

namespace NuLink.Lib.MsBuildFormat
{
    public class SlnFile
    {
        public SlnFile(FileInfo fileInfo)
        {
            this.FileInfo = fileInfo;
        }

        public FileInfo FileInfo { get; }
        public List<ISlnFilePart> Parts { get; } = new List<ISlnFilePart>();

        public void Save(TextWriter writer)
        {
            foreach (var part in Parts)
            {
                part.Save(writer);
            }
        }

        public FileInfo GetProjectFileInfo(ProjectSlnFilePart project)
        {
            return new FileInfo(Path.Combine(FileInfo.DirectoryName, project.SolutionRelativePath));
        }

        public string GetFullPath(params string[] relativePathParts)
        {
            return Path.Combine(FileInfo.DirectoryName, Path.Combine(relativePathParts));
        }

        public SlnSection FindGlobalSection(string name, SlnSectionKind kind)
        {
            return Parts
                .OfType<GlobalSlnFilePart>().Single()
                .Sections.Single(s => s.Name == name && s.Kind == kind);
        }

        public ProjectSlnFilePart AddExistingProject(string typeGuid, string projectFileRelativePath)
        {
            var projectItem = new ProjectSlnFilePart {
                Name = Path.GetFileNameWithoutExtension(projectFileRelativePath),
                SolutionRelativePath = projectFileRelativePath,
                TypeGuid = typeGuid,
                ProjectGuid = Guid.NewGuid().ToString("B").ToUpper()
            };

            var platformConfigSection = FindGlobalSection("ProjectConfigurationPlatforms", SlnSectionKind.PostSolution);

            platformConfigSection.Properties.Add(new PropertySlnFilePart {
                Name = $"{projectItem.ProjectGuid}.Debug|Any CPU.ActiveCfg",
                Value = "Debug|Any CPU"
            });
            platformConfigSection.Properties.Add(new PropertySlnFilePart {
                Name = $"{projectItem.ProjectGuid}.Debug|Any CPU.Build.0",
                Value = "Debug|Any CPU"
            });
            platformConfigSection.Properties.Add(new PropertySlnFilePart {
                Name = $"{projectItem.ProjectGuid}.Release|Any CPU.ActiveCfg",
                Value = "Release|Any CPU"
            });
            platformConfigSection.Properties.Add(new PropertySlnFilePart {
                Name = $"{projectItem.ProjectGuid}.Release|Any CPU.Build.0",
                Value = "Release|Any CPU"
            });

            var projectItemIndex = Parts.FindLastIndex(item => item is ProjectSlnFilePart);
            if (projectItemIndex < 0)
            {
                projectItemIndex = Parts.FindIndex(item => item is GlobalSlnFilePart);
            }

            Parts.Insert(projectItemIndex, projectItem);

            return projectItem;
        }

        public void RemoveProject(ProjectSlnFilePart projectItem)
        {
            if (!Parts.Remove(projectItem))
            {
                throw new Exception($"project is not listed in solution: {projectItem.Name}");
            }

            var platformConfigSection = FindGlobalSection("ProjectConfigurationPlatforms", SlnSectionKind.PostSolution);
            var propertiesToRemove = new HashSet<PropertySlnFilePart>(platformConfigSection.Properties.Where(p => p.Name.StartsWith(projectItem.ProjectGuid)));
            var remainingProperties = platformConfigSection.Properties.Where(p => !propertiesToRemove.Contains(p)).ToArray();

            platformConfigSection.Properties.Clear();
            platformConfigSection.Properties.AddRange(remainingProperties);
        }

    }
}
