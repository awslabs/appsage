using System.CommandLine;

namespace AppSage.Run.CommandSet.Root
{
    internal class AppSageRootCommand
    {
        public static Option<string> GetWorkspaceArgument()
        {
            var argWorkspaceFolder = new Option<string>(name: "--workspace-folder",aliases: new string[] { "-ws" });
            argWorkspaceFolder.Description = """
                The AppSage workspace folder path. If not specified, defaults to the current working directory.
                AppSage workspace is a special folder structure with some configuration. 
                If you wan to create a new workspace, please use the 'init' command first to initialize the folder.
                """;
            return argWorkspaceFolder;
        }
    }
}
