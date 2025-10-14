namespace AppSage.Extension
{
    public interface IExtensionManager
    {
        Task<IEnumerable<IExtension>> LoadExtensionsAsync();
        Task<IExtension?> LoadExtensionAsync(string extensionPath);
        Task UnloadExtensionAsync(string extensionId);
        IExtension? GetExtension(string extensionId);
        IEnumerable<IExtension> GetExtensions();
        Task<bool> InstallExtensionAsync(string packagePath);
        Task<bool> UninstallExtensionAsync(string extensionId);
    }
}