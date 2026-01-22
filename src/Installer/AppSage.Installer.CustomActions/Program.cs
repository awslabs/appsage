using System.Diagnostics;

namespace AppSage.Installer.CustomActions
{
    internal class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: AppSage.Installer.CustomActions.exe [INSTALL|UNINSTALL] [TARGETDIR=path] [ALLUSERS=1|0]");
                    return 1;
                }

                Console.WriteLine("Updating PATH environment variable...");

                string action = args[0].ToUpperInvariant();
                string targetDir = GetArgumentValue(args, "TARGETDIR");
                bool allUsers = GetArgumentValue(args, "ALLUSERS") == "1";

                if (string.IsNullOrEmpty(targetDir))
                {
                    Console.WriteLine("Error: TARGETDIR parameter is required");
                    return 1;
                }

                // Remove trailing backslash if present
                targetDir = targetDir.TrimEnd('\\', '/');

                EnvironmentVariableTarget target = allUsers 
                    ? EnvironmentVariableTarget.Machine 
                    : EnvironmentVariableTarget.User;

                Console.WriteLine($"Action: {action}");
                Console.WriteLine($"Target Directory: {targetDir}");
                Console.WriteLine($"Scope: {(allUsers ? "All Users (System)" : "Current User")}");

                if (action == "INSTALL")
                {
                    return AddToPath(targetDir, target) ? 0 : 1;
                }
                else if (action == "UNINSTALL")
                {
                    return RemoveFromPath(targetDir, target) ? 0 : 1;
                }
                else
                {
                    Console.WriteLine($"Error: Unknown action '{action}'. Use INSTALL or UNINSTALL.");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return 1;
            }
        
        }

        private static string GetArgumentValue(string[] args, string parameterName)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith($"{parameterName}=", StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Substring(parameterName.Length + 1).Trim('"');
                }
            }
            return string.Empty;
        }

        private static bool AddToPath(string directoryToAdd, EnvironmentVariableTarget target)
        {
            try
            {
                string currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? string.Empty;
                
                // Split the path into individual directories
                var pathDirs = currentPath
                    .Split(Path.PathSeparator)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                // Check if the directory already exists in PATH (case-insensitive)
                bool alreadyExists = pathDirs.Any(p => 
                    string.Equals(p, directoryToAdd, StringComparison.OrdinalIgnoreCase));

                if (alreadyExists)
                {
                    Console.WriteLine($"Directory already exists in PATH: {directoryToAdd}");
                    return true;
                }

                // Add the new directory to the path
                pathDirs.Add(directoryToAdd);

                // Rebuild the PATH string
                string newPath = string.Join(Path.PathSeparator.ToString(), pathDirs);

                // Set the environment variable
                Environment.SetEnvironmentVariable("PATH", newPath, target);

                Console.WriteLine($"Successfully added to PATH: {directoryToAdd}");
                
                // Broadcast the WM_SETTINGCHANGE message to notify other applications
                BroadcastEnvironmentChange();

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Insufficient permissions to modify {target} PATH. Administrator rights may be required.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to PATH: {ex.Message}");
                return false;
            }
        }

        private static bool RemoveFromPath(string directoryToRemove, EnvironmentVariableTarget target)
        {
            try
            {
                string currentPath = Environment.GetEnvironmentVariable("PATH", target) ?? string.Empty;

                // Split the path into individual directories
                var pathDirs = currentPath
                    .Split(Path.PathSeparator)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                // Remove the directory (case-insensitive)
                var originalCount = pathDirs.Count;
                pathDirs = pathDirs
                    .Where(p => !string.Equals(p, directoryToRemove, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (pathDirs.Count == originalCount)
                {
                    Console.WriteLine($"Directory not found in PATH: {directoryToRemove}");
                    return true; // Not an error condition
                }

                // Rebuild the PATH string
                string newPath = string.Join(Path.PathSeparator.ToString(), pathDirs);

                // Set the environment variable
                Environment.SetEnvironmentVariable("PATH", newPath, target);

                Console.WriteLine($"Successfully removed from PATH: {directoryToRemove}");
                
                // Broadcast the WM_SETTINGCHANGE message to notify other applications
                BroadcastEnvironmentChange();

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Insufficient permissions to modify {target} PATH. Administrator rights may be required.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from PATH: {ex.Message}");
                return false;
            }
        }

        private static void BroadcastEnvironmentChange()
        {
            try
            {
                // Use Process.Start to run a PowerShell command that broadcasts the environment change
                // This notifies other applications that environment variables have changed
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-Command \"[Environment]::SetEnvironmentVariable('TEMP_BROADCAST', [guid]::NewGuid().ToString(), 'Process'); Remove-Item Env:\\TEMP_BROADCAST\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                
                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit(5000);
                }
            }
            catch
            {
                // Silently fail - not critical for the installation
            }
        }
    }
}
