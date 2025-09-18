using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Run.CommandSet.Root
{
    internal class AppSageRootCommand
    {
        public static Option<string> GetWorkspaceArgument()
        {
            var argWorkspaceFolder = new Option<string>(name: "--workspace-folder",aliases: new string[] { "-ws" });
            argWorkspaceFolder.Description = $"The workspace folder path to initialize. This folder should be non existing or if exists should be empty.  If not specified, defaults to the current working directory. Which is as of now is [{Environment.CurrentDirectory}].";
            return argWorkspaceFolder;
        }
    }
}
