using NuLink.Lib.Abstractions;
using NuLink.Lib.MsBuildFormat;

namespace NuLink.Lib.Workspaces
{
    public class WorkspaceLoader : IWorkspaceLoader
    {
        private readonly IUserInterface _ui;
        private readonly JsonConfigPersistor _configPersistor;
        private readonly SlnFilePersistor _slnPersistor;

        public WorkspaceLoader(
            IUserInterface ui, 
            JsonConfigPersistor configPersistor,
            SlnFilePersistor slnPersistor)
        {
            _ui = ui;
            _configPersistor = configPersistor;
            _slnPersistor = slnPersistor;
        }

        public Workspace Load(CommandOptions options)
        {
            _ui.ReportImportant(() => $"Loading workspace, solution {options.Solution.FullName}");

            var slnFile = _slnPersistor.Load(options.Solution);
            var solution = new Solution(slnFile);
            var configuration = (options.Configuration.Exists 
                ? _configPersistor.Load(options.Configuration.FullName) 
                : new Configuration());
            
            return new Workspace(solution, configuration);
        }
    }
}
