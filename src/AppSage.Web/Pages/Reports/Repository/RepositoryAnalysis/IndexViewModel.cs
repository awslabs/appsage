using AppSage.Core.ComplexType;
using AppSage.Web.Models.Shared;

namespace AppSage.Web.Pages.Reports.Repository.RepositoryAnalysis
{
    public class IndexViewModel
    {
        public AnnotatedValue<int> RepositoryCount { get; set; } = new();
        public int FileCount { get; set; } = 0;
        public AnnotatedValue<int> LinesOfCode { get; set; } = new();
        public AnnotatedValue<int> DatabaseCount { get; set; } = new();
        public AnnotatedValue<int> ContributorCount { get; set; } = new();
        public AnnotatedValue<int> BranchCount { get; set; } = new();
        public AnnotatedValue<string> FirstCommitDate { get; set; } = new();
        public AnnotatedValue<string> LastCommitDate { get; set; } = new();
        public XYSeries<string,int> CommitSummary { get; set; } = new();
        public List<(string Name,int CommitCount)> TopContributors { get; set; } = new();

        public List<(string Category,string SubCategory, int Count)> TechnologyDistribution { get; set; } = new();
        public string TechDataMetricTableName { get; set; } = string.Empty;

        public List<RepositoryTimelineData> RepositoryTimelines { get; set; } = new();

        public List<RepositoryDetail> Repositories { get; set; } = new List<RepositoryDetail>();
    }

    public class RepositoryTimelineData
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<int> CommitCounts { get; set; } = new List<int>();
    }

 

    public class RepositoryDetail
    {
        public string Name { get; set; } = string.Empty;
        public DateTime FirstCommitDate { get; set; }
        public DateTime LastCommitDate { get; set; }
        public int TotalCommits { get; set; }
        public int ContributorCount { get; set; }
        public int BranchCount { get; set; }
        public List<string> CommitMonths { get; set; } = new List<string>();
        public List<int> CommitCounts { get; set; } = new List<int>();
    }

  
}