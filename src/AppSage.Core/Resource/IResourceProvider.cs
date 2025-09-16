namespace AppSage.Core.Resource
{
    public interface IResourceProvider
    {
        IEnumerable<IResource> GetResources();
    }
}
