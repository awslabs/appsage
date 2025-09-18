namespace AppSage.Core.Workspace
{
    public interface IAppSageWorkspace
    {
        //input folders
        protected const string REPOSITORIES_ROOT_FOLDER_NAME = "Repositories";
        protected const string DATABASE_SCHEMA_ROOT_FOLDER_NAME = "DatabaseSchemas";

        //output folders
        protected const string OUTPUT_ROOT_FOLDER_NAME = "Output";
        protected const string PROVIDER_OUTPUT_FOLDER_NAME = "Provider";
        protected const string MCP_SERVER_OUTPUT_FOLDER_NAME = "MCPServer";
        protected const string LOGS_ROOT_FOLDER_NAME = "Logs";

        //Provider related folders
        protected const string PROVIDER_ROOT_FOLDER_NAME = "Providers";

        //Hidden cache folder
        protected const string CACHE_ROOT_FOLDER_NAME = ".Cache";

        //Hidden AppSage config folder
        protected const string APPSAGE_CONFIG_ROOT_FOLDER_NAME = ".AppSageConfig";

        string RootFolder { get; }

        string RepositoryFolder => Path.Combine(RootFolder, REPOSITORIES_ROOT_FOLDER_NAME);

        string DatabaseSchemaFolder => Path.Combine(RootFolder, DATABASE_SCHEMA_ROOT_FOLDER_NAME);

        string ProviderOutputFolder => Path.Combine(RootFolder, OUTPUT_ROOT_FOLDER_NAME, PROVIDER_OUTPUT_FOLDER_NAME);

        string MCPServerOutputFolder => Path.Combine(RootFolder, OUTPUT_ROOT_FOLDER_NAME, MCP_SERVER_OUTPUT_FOLDER_NAME);

        string LogsFolder => Path.Combine(RootFolder, LOGS_ROOT_FOLDER_NAME);

        string ProviderFolder => Path.Combine(RootFolder, PROVIDER_ROOT_FOLDER_NAME);

        string AppSageConfigFolder => Path.Combine(RootFolder, APPSAGE_CONFIG_ROOT_FOLDER_NAME);
        string CacheFolder => Path.Combine(RootFolder, CACHE_ROOT_FOLDER_NAME);

        

        string GetResourceName(string path);

        string GetRepositoryName(string path);
    }
}
