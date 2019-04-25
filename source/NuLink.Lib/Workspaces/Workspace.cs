namespace NuLink.Lib.Workspaces
{
    public class Workspace
    {
        public Workspace(Solution solution, Configuration configuration)
        {
            Solution = solution;
            Configuration = configuration;
        }

        public Solution Solution { get; }
        public Configuration Configuration { get; }
    }
}
