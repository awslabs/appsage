namespace AppSage.Extension
{
    public interface IExtensionManager
    {
        IEnumerable<IExtension> GetExtensions();
        bool InstallExtension(string extensionId);
        bool UninstallExtension(string extensionId);
    }
}