using NuLink.Lib.Abstractions;
using NuLink.Lib.MsBuildFormat;

namespace NuLink.Lib.Workspaces
{
    public class WorkspaceLoader : IWorkspaceLoader
    {
        private readonly IUserInterface _ui;
        private readonly IImmutableEnvironment _environment;
        private readonly ISourceControl _sourceControl;
        private readonly JsonConfigPersistor _configPersistor;
        private readonly SlnFilePersistor _slnPersistor;

        public WorkspaceLoader(
            IUserInterface ui, 
            IImmutableEnvironment environment,
            ISourceControl sourceControl,
            JsonConfigPersistor configPersistor,
            SlnFilePersistor slnPersistor)
        {
            _ui = ui;
            _environment = environment;
            _sourceControl = sourceControl;
            _configPersistor = configPersistor;
            _slnPersistor = slnPersistor;
        }

        public Workspace Load(CommandOptions options)
        {
            _ui.ReportImportant(() => $"Loading workspace, solution {options.Solution.FullName}");

            var slnFile = _slnPersistor.Load(options.Solution);
            var solution = new Solution(_environment, _sourceControl, slnFile);
            var configuration = (options.Configuration.Exists 
                ? _configPersistor.Load(options.Configuration.FullName) 
                : new Configuration());
            
            return new Workspace(solution, configuration);
        }
    }
}
