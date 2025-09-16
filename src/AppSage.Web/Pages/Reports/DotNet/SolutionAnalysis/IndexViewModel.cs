using System.Data;

namespace AppSage.Web.Pages.Reports.DotNet.SolutionAnalysis
{

    public class IndexViewModel
    {
        public List<(string Title, string Description, DataExportName)> DataExports { get; set; } = new();

        public List<ProjectMetrics> Projects { get; set; } = new();

        public string ReferencesMetricTableName { get; set; } = string.Empty;
        public SolutionSummary Summary { get; set; } = new();
    }


    public class ProjectMetrics
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty;
        public string DotNetVersion { get; set; } = string.Empty;
        public int DocumentCount { get; set; }
        public int ClassCount { get; set; }
        public int MethodCount { get; set; }
        public int DataClassCount { get; set; }
        public DataTable? NugetPackages { get; set; }
        public string Segment { get; set; } = string.Empty;
    }

    public class SolutionSummary
    {
        public int TotalProjects { get; set; }
        public int TotalClasses { get; set; }
        public int TotalMethods { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalDataClasses { get; set; }

        public Dictionary<string, int> LanguageDistribution { get; set; } = new();
        public Dictionary<string, int> ProjectTypeDistribution { get; set; } = new();
        public Dictionary<string, int> DotNetVersionDistribution { get; set; } = new();
        public List<(string PackageName, int UsageCount)> TopNugetPackages { get; set; } = new();
    }
}
