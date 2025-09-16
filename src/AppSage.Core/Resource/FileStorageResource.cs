namespace AppSage.Core.Resource
{
    public class FileStorageResource : IResource
    {
        public string Name { get; }
        public string Path { get; }
        public FileStorageResource(string name, string path)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        
    }
}
