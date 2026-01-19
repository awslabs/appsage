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
        protected const string QUERY_TEMPLATES_ROOT_FOLDER_NAME = "QueryTemplates";

        //output folders
        protected const string OUTPUT_ROOT_FOLDER_NAME = "Output";
        protected const string PROVIDER_OUTPUT_FOLDER_NAME = "Provider";
        protected const string MCP_SERVER_OUTPUT_FOLDER_NAME = "MCPServer";
        protected const string QUERY_OUTPUT_FOLDER_NAME = "Query";
        protected const string LOGS_ROOT_FOLDER_NAME = "Logs";

        //Extensions related folders
        protected const string EXTENSION_ROOT_FOLDER_NAME = "Extensions";
        protected const string EXTENSION_PACKAGE_FOLDER_NAME = "Packages";
        protected const string EXTENSION_INSTALL_FOLDER_NAME = "Installed";
        protected const string EXTENSION_DOCUMENTATION_FOLDER_NAME = "Docs";

        //Hidden cache folder
        protected const string CACHE_ROOT_FOLDER_NAME = ".Cache";

        //Hidden AppSage config folder
        protected const string APPSAGE_CONFIG_ROOT_FOLDER_NAME = ".AppSageConfig";
        protected const string APPSAGE_CONFIG_FILENAME = "AppSageConfig.json";


        string RootFolder { get; }

        string RepositoryFolder => Path.Combine(RootFolder, REPOSITORIES_ROOT_FOLDER_NAME);

        string ProviderOutputFolder => Path.Combine(RootFolder, OUTPUT_ROOT_FOLDER_NAME, PROVIDER_OUTPUT_FOLDER_NAME);

        string MCPServerOutputFolder => Path.Combine(RootFolder, OUTPUT_ROOT_FOLDER_NAME, MCP_SERVER_OUTPUT_FOLDER_NAME);

        string TemplateBasedAnalysisOutputFolder => Path.Combine(RootFolder, OUTPUT_ROOT_FOLDER_NAME, QUERY_OUTPUT_FOLDER_NAME);

        string TemplateFolder => Path.Combine(RootFolder, QUERY_TEMPLATES_ROOT_FOLDER_NAME);

        string LogsFolder => Path.Combine(RootFolder, LOGS_ROOT_FOLDER_NAME);

        string ExtensionFolder => Path.Combine(RootFolder, EXTENSION_ROOT_FOLDER_NAME);
        string ExtensionPackagesFolder => Path.Combine(ExtensionFolder, EXTENSION_PACKAGE_FOLDER_NAME);
        string ExtensionInstallFolder => Path.Combine(ExtensionFolder, EXTENSION_INSTALL_FOLDER_NAME);

        /// <summary>
        /// Get the documentation folder for a specific extension
        /// </summary>
        /// <param name="extensionId">Full qualified extension name</param>
        /// <returns></returns>
        string GetExtensionDocumentationFolder(string extensionId)
        {
            return Path.Combine(ExtensionInstallFolder, extensionId, EXTENSION_DOCUMENTATION_FOLDER_NAME);
        }



        string AppSageConfigFolder => Path.Combine(RootFolder, APPSAGE_CONFIG_ROOT_FOLDER_NAME);
        string AppSageConfigFilePath => Path.Combine(AppSageConfigFolder, APPSAGE_CONFIG_FILENAME);
        string CacheFolder => Path.Combine(RootFolder, CACHE_ROOT_FOLDER_NAME);
    }
}
