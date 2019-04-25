using NuLink.Lib.Workspaces;

namespace NuLink.Lib.Abstractions
{
    public interface IWorkspaceLoader
    {
        Workspace Load(CommandOptions options);
    }
}
