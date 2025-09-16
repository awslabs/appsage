using System.Data;

namespace AppSage.Web.Components.DataExport
{
    public class DataExportModel
    {
        public string DataExportId { get; set; } = Guid.NewGuid().ToString("N");
        public DataTable Table { get; set; } = new DataTable();
        public string Title { get; set; } = string.Empty;
    }
}
