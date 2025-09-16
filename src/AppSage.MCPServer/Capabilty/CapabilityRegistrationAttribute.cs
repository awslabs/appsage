using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
namespace AppSage.MCPServer.Capabilty
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CapabilityRegistrationAttribute : Attribute
    {
        const string _DEFAULT_NAMEPREFIX = "AppSage";
        const string _DEFAULT_ROOT = "CapabilityGuide";

        public string Name { get; }
        public string GuideFolderPath { get; }
        public CapabilityRegistrationAttribute(string name, string guideFolderPath)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                throw new ArgumentNullException(nameof(name), "Name cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(guideFolderPath) || string.IsNullOrEmpty(guideFolderPath.Trim()))
            {
                throw new ArgumentNullException(nameof(name), "guideFolderPath cannot be null or empty.");
            }

            // Alphanumeric validation
            if (!Regex.IsMatch(name.Trim(), "^[a-zA-Z0-9]+$"))
            {
                throw new ArgumentException("Name must contain only alphanumeric characters (A-Z, a-z, 0-9).", nameof(name));
            }

            var path = guideFolderPath.Trim();
            path = path.StartsWith("\\") ? path.Substring(1) : path; // remove leading slash
            path = path.EndsWith("\\") ? path.Substring(0, path.Length - 1) : path; // remove trailing slash
            GuideFolderPath = Path.Combine(_DEFAULT_ROOT, path);

            Name=$"{_DEFAULT_NAMEPREFIX}_{name.Trim()}";

        }

        public string GetCapabiltyKey(MethodInfo method)
        {
            return $"{Name}_{method.Name}";
        }


    }
}
