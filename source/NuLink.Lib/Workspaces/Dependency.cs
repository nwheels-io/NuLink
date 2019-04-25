namespace NuLink.Lib.Workspaces
{
    public class Dependency
    {
        public Dependency(Project principal, Project dependent)
        {
            Principal = principal;
            Dependent = dependent;
        }

        public Project Principal { get; }
        public Project Dependent { get; }
    }
}
