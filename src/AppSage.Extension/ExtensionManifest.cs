using System.Text.Json.Serialization;

namespace AppSage.Extension
{
    public class ExtensionManifest
    {
        [JsonPropertyName("ExtensionId")]
        public string ExtensionId { get; set; } = "";

        [JsonPropertyName("Version")]
        public string Version { get; set; } = "";

        [JsonPropertyName("DisplayName")]
        public string DisplayName { get; set; } = "";

        [JsonPropertyName("Description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("Author")]
        public string Author { get; set; } = "";

        [JsonPropertyName("EntryAssembly")]
        public string EntryAssembly { get; set; } = "";

        [JsonPropertyName("TargetFramework")]
        public string TargetFramework { get; set; } = "";

        [JsonPropertyName("HostVersion")]
        public string HostVersion { get; set; } = "";

        [JsonPropertyName("Dependencies")]
        public ExtensionDependencies Dependencies { get; set; } = new();

    }

    public class ExtensionDependencies
    {
        [JsonPropertyName("Bundled")]
        public BundledDependency[] Bundled { get; set; } = Array.Empty<BundledDependency>();

        [JsonPropertyName("HostProvided")]
        public HostProvidedDependency[] HostProvided { get; set; } = Array.Empty<HostProvidedDependency>();

        [JsonPropertyName("External")]
        public ExternalDependency[] External { get; set; } = Array.Empty<ExternalDependency>();
    }

    public class BundledDependency
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("Version")]
        public string Version { get; set; } = "";

        [JsonPropertyName("Assemblies")]
        public string[] Assemblies { get; set; } = Array.Empty<string>();
    }

    public class HostProvidedDependency
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("Version")]
        public string Version { get; set; } = "";
    }

    public class ExternalDependency
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("Version")]
        public string Version { get; set; } = "";

        [JsonPropertyName("Source")]
        public string Source { get; set; } = "";

        [JsonPropertyName("Optional")]
        public bool Optional { get; set; }
    }

    public class ExtensionConfiguration
    {
        [JsonPropertyName("RequiresConfiguration")]
        public bool RequiresConfiguration { get; set; }

        [JsonPropertyName("ConfigurationSchema")]
        public string ConfigurationSchema { get; set; } = "";
    }
}