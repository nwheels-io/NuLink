namespace NuLink.Cli
{
    public interface INuLinkCommand
    {
        void Execute(NuLinkCommandOptions options);
    }
}
