using AppSage.Core.Logging;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace AppSage.Providers.DotNet.Utility
{
    internal class AssemblyAnalyzer
    {
        bool _cacheResult = false;
        IAppSageLogger _logger;
        Dictionary<string, AssemblyInfo> _cache;
        public AssemblyAnalyzer(IAppSageLogger logger, bool cacheResult)
        {
            _logger = logger;
            _cacheResult = cacheResult;
            _cache = new Dictionary<string, AssemblyInfo>();
        }

        public AssemblyInfo GetAssemblyInfo(string path)
        {
            var result = new AssemblyInfo();

            if (string.IsNullOrEmpty(path))
            {
                result.Path = ConstString.UNKNOWN;
                result.Name = ConstString.UNKNOWN;
                result.Version = ConstString.UNKNOWN;
                result.TargetFramework = ConstString.UNKNOWN;
                return result;
            }

            if (_cacheResult && _cache.ContainsKey(path))
            {
                return _cache[path];
            }


            result.Path = path;

            try
            {
                result.Name = Path.GetFileNameWithoutExtension(path);
            }
            catch
            {
                _logger.LogWarning($"Failed to get file name for assembly at path {path}");
                result.Name = ConstString.UNKNOWN;
            }

            if (File.Exists(path))
            {

                result.Version = ConstString.UNKNOWN;
                result.TargetFramework = ConstString.UNKNOWN;

                try
                {
                    var assemblyVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
                    if (assemblyVersionInfo != null && !string.IsNullOrEmpty(assemblyVersionInfo.FileVersion))
                    {
                        result.Version = assemblyVersionInfo.FileVersion;
                    }
                }
                catch (Exception ex)
                {
                    result.Version = ConstString.UNKNOWN;
                    _logger.LogWarning($"Failed to get version info for assembly {result.Name}:{ex.Message}");
                }
                try
                {
                    result.TargetFramework = GetTargetFramework(path);

                }
                catch (Exception ex)
                {
                    result.TargetFramework = ConstString.UNKNOWN;
                    _logger.LogWarning($"Failed to get file version info for assembly {result.Name}: {ex.Message}");
                }
            }
            else
            {
                result.Version = ConstString.UNKNOWN;
                result.TargetFramework = ConstString.UNKNOWN;
            }

            if (_cacheResult)
            {
                if (!_cache.ContainsKey(path))
                {
                    _cache.Add(path, result);
                }
            }

            return result;

        }

        private static string GetTargetFramework(string assemblyPath)
        {
            try
            {
                using var stream = File.OpenRead(assemblyPath);
                using var peReader = new PEReader(stream);

                if (!peReader.HasMetadata)
                    return ConstString.UNKNOWN;

                var reader = peReader.GetMetadataReader();

                foreach (var handle in reader.GetCustomAttributes(EntityHandle.ModuleDefinition))
                {
                    var attribute = reader.GetCustomAttribute(handle);
                    var ctorHandle = attribute.Constructor;
                    string? attributeType = null;

                    if (ctorHandle.Kind == HandleKind.MemberReference)
                    {
                        var memberRef = reader.GetMemberReference((MemberReferenceHandle)ctorHandle);
                        var parent = memberRef.Parent;
                        if (parent.Kind == HandleKind.TypeReference)
                        {
                            var typeRef = reader.GetTypeReference((TypeReferenceHandle)parent);
                            attributeType = reader.GetString(typeRef.Namespace) + "." + reader.GetString(typeRef.Name);
                        }
                    }

                    if (attributeType == "System.Runtime.Versioning.TargetFrameworkAttribute")
                    {
                        // Get a BlobReader to read the attribute value
                        var blobReader = reader.GetBlobReader(attribute.Value);
                        blobReader.ReadUInt16(); // skip prolog
                        return blobReader.ReadSerializedString();
                    }
                }
            }
            catch (Exception ex)
            {
                // Return unknown on any exceptions rather than propagating them
                return ConstString.UNKNOWN;
            }

            return ConstString.UNKNOWN;
        }

    }



    public record AssemblyInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string TargetFramework { get; set; }
    }
}
