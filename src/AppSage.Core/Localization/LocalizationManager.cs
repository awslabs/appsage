using System.Reflection;
using System.Resources;

namespace AppSage.Core.Localization
{
    public class LocalizationManager
    {
        private static readonly Dictionary<Type, LocalizationManager> _instances = new Dictionary<Type, LocalizationManager>();
        private static bool _initialized = false;
        private ResourceManager _resourceManager = null!;

        /// <summary>
        /// Static initializer for LocalizationManager
        /// </summary>
        public static void InitializeAll()
        {
            if (_initialized) { return; }

            // Get all assemblies in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .ToList();

            // Find all LocalizationManager derived types
            foreach (var assembly in assemblies)
            {

                var types = assembly.GetTypes()
                    .Where(t => t != typeof(LocalizationManager) &&
                           typeof(LocalizationManager).IsAssignableFrom(t))
                    .ToList();

                foreach (var type in types)
                {
                    // Create instance if it has parameterless constructor
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        var instance = (LocalizationManager)constructor.Invoke(null);
                        _instances[type] = instance;
                    }
                }
            }
            _initialized = true;
        }

        /// <summary>
        /// Initializes the localization system with the embedded resource manager
        /// </summary>
        public LocalizationManager(string baseName)
        {
            Type type = this.GetType();
            _resourceManager = new ResourceManager(baseName, type.Assembly);
            RegisterAllTypes(type);
        }

        /// <summary>
        /// Registers all public types that need localization
        /// </summary>
        private void RegisterAllTypes(Type rootType)
        {
            Stack<Type> types = new Stack<Type>();
            types.Push(rootType);
            while (types.Count > 0)
            {
                Type type = types.Pop();
                // Register the type for localization
                ProcessStaticStringProperties(type);
                // Get all public nested types and add them to the stack to be processed
                foreach (var nestedType in type.GetNestedTypes())
                {
                    types.Push(nestedType);
                }
            }
        }

        /// <summary>
        /// Processes all static string properties in a type and sets up dynamic localization
        /// </summary>
        private void ProcessStaticStringProperties(Type type)
        {
            // Find all public static string properties/fields
            var stringProperties = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(string));

            foreach (var property in stringProperties)
            {
                // Get the default value to use as fallback
                string defaultValue = (string)property.GetValue(null);

                // Create a resource key using the fully qualified name
                string resourceKey = $"{type.FullName}.{property.Name}";

                string localizedValue = GetLocalizedString(resourceKey, defaultValue);
                property.SetValue(null, localizedValue);
            }
        }

        /// <summary>
        /// Gets a localized string for the given resource key
        /// </summary>
        private string GetLocalizedString(string resourceKey, string defaultValue)
        {
            if (_resourceManager == null)
            {
                return defaultValue;
            }

            try
            {
                string result = _resourceManager.GetString(resourceKey, Thread.CurrentThread.CurrentUICulture);
                return string.IsNullOrEmpty(result) ? defaultValue : result;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
