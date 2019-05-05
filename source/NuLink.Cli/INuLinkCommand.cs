namespace NuLink.Cli
{
    public interface INuLinkCommand
    {
        int Execute(NuLinkCommandOptions options);
    }
}
