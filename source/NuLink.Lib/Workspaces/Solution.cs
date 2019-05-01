using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NuLink.Lib.Abstractions;
using NuLink.Lib.MsBuildFormat;

namespace NuLink.Lib.Workspaces
{
    public class Solution
    {
        public Solution(
            IImmutableEnvironment environment,
            ISourceControl sourceControl,
            SlnFile sln)
        {
            this.Sln = sln;
            this.Projects = sln.Parts
                .OfType<ProjectSlnFilePart>()
                .Where(item => item.IsCSharpProject())
                .Select(p => new Project(environment, sourceControl, this, p))
                .ToList();
            
            this.ProjectByName = Projects.ToImmutableDictionary(p => p.Name);

            PopulateDependencyLists();
        }
        
        public SlnFile Sln { get; }
        public IReadOnlyList<Project> Projects { get; }
        public IReadOnlyDictionary<string, Project> ProjectByName { get; }

        private void PopulateDependencyLists()
        {
            var allDepsInSolution = new HashSet<Dependency>();

            foreach (var project in ProjectByName.Values)
            {
                project.PopulateDependencyProjects();
                allDepsInSolution.UnionWith(project.DependencyProjects);
            }

            foreach (var project in ProjectByName.Values)
            {
                project.PopulateDependentProjects(allDepsInSolution);
            }
        }
    }
}
