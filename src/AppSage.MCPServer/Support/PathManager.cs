namespace AppSage.McpServer.Support;

public static class PathManager
{
    public static string? GetFullDirectoryPath(string relative)
    {
        var p1 = Path.GetFullPath(relative);
        if (Directory.Exists(p1)) return p1;

        var p2 = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relative));
        return Directory.Exists(p2) ? p2 : null;
    }
}
