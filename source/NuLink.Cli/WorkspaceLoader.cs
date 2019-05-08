using System.Collections.Generic;
using Buildalyzer;

namespace NuLink.Cli
{
    public class WorkspaceLoader
    {
        public IEnumerable<ProjectAnalyzer> LoadProjects(string filePath, bool isSolution)
        {
            var analyzerManager = (isSolution 
                ? new AnalyzerManager(filePath) 
                : new AnalyzerManager());

            if (isSolution)
            {
                return analyzerManager.Projects.Values;
            }
            else
            {
                return new[] { analyzerManager.GetProject(filePath) };
            }
        }
    }
}