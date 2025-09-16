namespace AppSage.Core.Workspace
{
    public interface IAppSageWorkspace
    {
        string RootFolder { get; }
        string RepositoryFolder { get; }
        string DatabaseSchemaFolder { get; }

        string CacheFolder { get; }

        string GetResourceName(string path);

        string GetRepositoryName(string path);
    }
}
