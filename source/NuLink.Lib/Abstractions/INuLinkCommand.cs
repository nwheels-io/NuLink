namespace NuLink.Lib.Abstractions
{
    public interface INuLinkCommand
    {
        void Execute(CommandOptions options);
    }
}
