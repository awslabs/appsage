namespace AppSage.Core.Workspace
{
    public interface IAppSageWorkspace: IAppSageWorkspacePaths
    {
        string GetResourceName(string path);

        string GetRepositoryName(string path);
    }

    public interface IAppSageWorkspacePaths
    {
        //input folders
        protected const string REPOSITORIES_ROOT_FOLDER_NAME = "Repositories";
        protected const string DOCS_ROOT_FOLDER_NAME = "Docs";

        //output folders
        protected const string OUTPUT_ROOT_FOLDER_NAME = "Output";
        protected const string PROVIDER_OUTPUT_FOLDER_NAME = "Provider";
        protected const string MCP_SERVER_OUTPUT_FOLDER_NAME = "MCPServer";
        protected const string LOGS_ROOT_FOLDER_NAME = "Logs";

        //Extensions related folders
        protected const string EXTENSION_ROOT_FOLDER_NAME = "Extensions";

        //Hidden cache folder
        protected const string CACHE_ROOT_FOLDER_NAME = ".Cache";

        //Hidden AppSage config folder
        protected const string APPSAGE_CONFIG_ROOT_FOLDER_NAME = ".AppSageConfig";
        protected const string APPSAGE_CONFIG_FILENAME = "AppSageConfig.json";

        string RootFolder { get; }

        string RepositoryFolder => Path.Combine(RootFolder, REPOSITORIES_ROOT_FOLDER_NAME);

        string ProviderOutputFolder => Path.Combine(RootFolder, OUTPUT_ROOT_FOLDER_NAME, PROVIDER_OUTPUT_FOLDER_NAME);

        string MCPServerOutputFolder => Path.Combine(RootFolder, OUTPUT_ROOT_FOLDER_NAME, MCP_SERVER_OUTPUT_FOLDER_NAME);

        string LogsFolder => Path.Combine(RootFolder, LOGS_ROOT_FOLDER_NAME);

        string ExtensionFolder => Path.Combine(RootFolder, EXTENSION_ROOT_FOLDER_NAME);

        string DocsFolder => Path.Combine(RootFolder, DOCS_ROOT_FOLDER_NAME);

        string AppSageConfigFolder => Path.Combine(RootFolder, APPSAGE_CONFIG_ROOT_FOLDER_NAME);
        string AppSageConfigFilePath => Path.Combine(AppSageConfigFolder, APPSAGE_CONFIG_FILENAME);
        string CacheFolder => Path.Combine(RootFolder, CACHE_ROOT_FOLDER_NAME);
    }
}
