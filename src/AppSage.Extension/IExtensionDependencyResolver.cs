using System.Reflection;

namespace AppSage.Extension
{
    public interface IExtensionDependencyResolver
    {
        Assembly? ResolveHostDependency(AssemblyName assemblyName, HostProvidedDependency hostDependency);
        Task<Assembly?> ResolveExternalDependencyAsync(AssemblyName assemblyName, ExternalDependency externalDependency);
        Task<ValidationResult> ValidateDependenciesAsync(ExtensionManifest manifest);
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}