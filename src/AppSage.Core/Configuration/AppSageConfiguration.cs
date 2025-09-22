using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace AppSage.Core.Configuration
{
    public class AppSageConfiguration : IAppSageConfiguration,IAppSageConfigurationWriter
    {
        private IConfiguration _configuraiton;
        private string _configFilePath;

        [ActivatorUtilitiesConstructor]
        public AppSageConfiguration(): this(GetDefaultConfigTemplateFilePath())
        {
            
        }

        public AppSageConfiguration(string configFilePath)
        {
            if (string.IsNullOrWhiteSpace(configFilePath))
            {
                throw new ArgumentException("Configuration file path cannot be null or empty.", nameof(configFilePath));
            }
            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Configuration file '{configFilePath}' not found.", configFilePath);
            }
            _configuraiton = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configFilePath) ?? throw new InvalidOperationException("Could not determine the directory of the configuration file."))
                .AddJsonFile(Path.GetFileName(configFilePath), optional: false, reloadOnChange: true)
                .Build();
            _configFilePath = configFilePath;
        }


        public static string GetDefaultConfigTemplateFilePath()
        {
            // Get the current assembly directory
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configDirectory = Path.Combine(directory, "Configuration");

#if DEBUG
            string configFileName = "appsettings.Development.json";
#elif RELEASE
            string configFileName = "appsettings.Production.json";
#else
            throw new InvalidOperationException("Unknown build configuration. Please define DEBUG or RELEASE. Make sure you have the correct configuration file.");
#endif
            var configFilePath = Path.Combine(configDirectory, configFileName);
            if(!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Default configuration file '{configFilePath}' not found. Ensure that the configuration file exists in the expected location.", configFilePath);
            }

            return configFilePath;
        }

        public bool KeyExist(string key)
        {
            var section = _configuraiton.GetSection(key);
            return section.Exists();
        }

        public T Get<T>(string key)
        {
            var section = _configuraiton.GetSection(key);
            if (section.Exists())
            {
                return section.Get<T>();
            }
            throw new KeyNotFoundException($"Configuration key '{key}' not found.");
        }

        public void Set<T>(string key, T value)
        {
            try
            {
                // Ensure the configuration directory exists
                string configDirectory = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }

                JObject jsonConfig;

                // Read existing configuration file or create new one
                if (File.Exists(_configFilePath))
                {
                    string jsonText = File.ReadAllText(_configFilePath);
                    jsonConfig = string.IsNullOrWhiteSpace(jsonText) ? new JObject() : JObject.Parse(jsonText);
                }
                else
                {
                    jsonConfig = new JObject();
                }

                // Split the key by ':' to handle nested configuration sections
                string[] keyParts = key.Split(':');
                JObject currentSection = jsonConfig;

                // Navigate/create nested sections
                for (int i = 0; i < keyParts.Length - 1; i++)
                {
                    string sectionName = keyParts[i];
                    if (currentSection[sectionName] == null)
                    {
                        currentSection[sectionName] = new JObject();
                    }
                    currentSection = (JObject)currentSection[sectionName];
                }

                // Set the value at the final key
                string finalKey = keyParts[keyParts.Length - 1];
                currentSection[finalKey] = JToken.FromObject(value);

                // Write the updated configuration back to file
                string updatedJson = jsonConfig.ToString(Formatting.Indented);
                File.WriteAllText(_configFilePath, updatedJson);

                // Reload the configuration to reflect changes
                _configuraiton = new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(_configFilePath))
                    .AddJsonFile(Path.GetFileName(_configFilePath), optional: true, reloadOnChange: true)
                    .Build();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set configuration key '{key}': {ex.Message}", ex);
            }
        }
    }
}
