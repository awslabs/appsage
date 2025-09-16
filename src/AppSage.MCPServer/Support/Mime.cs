namespace AppSage.MCPServer.Support;

public static class Mime
{
    public static string FromExtension(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension.ToLowerInvariant() switch
           {
               ".xlsx" or ".xlsm" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
               ".xls" => "application/vnd.ms-excel",
               ".json" => "application/json",
               ".pdf" => "application/pdf",
               ".csv" => "text/csv",
               ".txt" => "text/plain",
               ".xml" => "application/xml",
               ".zip" => "application/zip",
               ".png" => "image/png",
               ".jpg" or ".jpeg" => "image/jpeg",
               ".gif" => "image/gif",
               ".svg" => "image/svg+xml",
               _ => "application/octet-stream"
           };
    }

}
