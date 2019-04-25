using NuLink.Lib.Abstractions;

namespace NuLink.Lib.Workspaces
{
    public class WorkspaceLoader : IWorkspaceLoader
    {
        private readonly IUserInterface _ui;
        private readonly IImmutableEnvironment _environment;

        public WorkspaceLoader(IUserInterface ui, IImmutableEnvironment environment)
        {
            _ui = ui;
            _environment = environment;
        }

        public Workspace Load(CommandOptions options)
        {
            _ui.ReportImportant(() => $"Loading workspace, solution {options.Solution.FullName}");
            
            throw new System.NotImplementedException();
        }
    }
}
