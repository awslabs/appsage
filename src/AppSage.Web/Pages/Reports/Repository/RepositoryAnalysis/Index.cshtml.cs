using AppSage.Core.ComplexType;
using AppSage.Core.Configuration;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Providers.Repository;
using AppSage.Web.Components.Filter;
using System.Data;

namespace AppSage.Web.Pages.Reports.Repository.RepositoryAnalysis
{
    public class IndexModel : MetricFilterPageModel
    {
        private readonly IAppSageLogger _logger;

        public IndexViewModel Dashboard { get; set; }

        public IndexModel(IAppSageLogger logger, IAppSageConfiguration config, IAppSageWorkspace workspace) : base(logger, config, workspace) 
        {
            _logger = logger;
        }
        private void PopulateMetricBoxes(IEnumerable<IMetric> metrics, ref IndexViewModel model)
        {
            model.RepositoryCount.Value = metrics.Where(x => x.Name == MetricName.Repository.REPOSITORY_COUNT).Select(x => (IMetricValue<int>)x).Sum(x => x.Value);
            int gitRepoCount = metrics.Where(x => x.Name == MetricName.Repository.Git.REPOSITORY_COUNT).Select(x => (IMetricValue<int>)x).Sum(x => x.Value);
            model.RepositoryCount.Annotations.Add($"Total number of git repositories:{gitRepoCount}");
            model.RepositoryCount.Annotations.Add($"Other types of repositories: {model.RepositoryCount.Value - gitRepoCount}");

            model.FileCount = metrics.Where(metrics => metrics.Name == MetricName.Repository.FILE_COUNT).Select(x => (IMetricValue<int>)x).Sum(x => x.Value);
            model.LinesOfCode.Value = metrics.Where(metrics => metrics.Name == MetricName.Repository.LINES_OF_CODE_COUNT).Select(x => (IMetricValue<int>)x).Sum(x => x.Value);
            model.LinesOfCode.Annotations.Add($"Total number of lines of code is estimated by counting lines of code files."); model.LinesOfCode.Annotations.Add($"Binary files are ignored");

            model.ContributorCount.Value = metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.CONTRIBUTOR_COUNT).Select(x => (IMetricValue<int>)x).Sum(x => x.Value);
            model.ContributorCount.Annotations.Add($"Total number of contributors is estimated using git contributors");

            model.BranchCount.Value = metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.BRANCH_COUNT).Select(x => (IMetricValue<int>)x).Sum(x => x.Value);
            model.BranchCount.Annotations.Add($"Total number of branches is estimated using git branches");

            if (metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.FIRST_COMMIT_DATE).Any())
            {
                model.FirstCommitDate.Value = metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.FIRST_COMMIT_DATE).Select(x => (IResourceMetricValue<DateTimeOffset>)x).Min(x => x.Value).ToString("yyyy-MMM-dd");
                model.FirstCommitDate.Annotations.Add($"First commit date is estimated using git commit data");
            }
            else
            {
                model.FirstCommitDate.Value = "Can't estimate.";
            }
            if (metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.LAST_COMMIT_DATE).Any())
            {
                model.LastCommitDate.Value = metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.LAST_COMMIT_DATE).Select(x => (IResourceMetricValue<DateTimeOffset>)x).Max(x => x.Value).ToString("yyyy-MMM-dd");
                model.LastCommitDate.Annotations.Add($"Last commit date is estimated using git commit data");
            }
            else
            {
                model.LastCommitDate.Value = "Can't estimate.";
            }
        }



        private string ExportTableToCsv(DataTable table, IEnumerable<string>? columnNames = null)
        {
            var columns = columnNames != null && columnNames.Any()
                ? table.Columns.Cast<DataColumn>().Where(c => columnNames.Contains(c.ColumnName))
                : table.Columns.Cast<DataColumn>();

            var sb = new System.Text.StringBuilder();

            // Add headers
            sb.AppendLine(string.Join(",", columns.Select(c => $"\"{c.ColumnName}\"")));

            // Add rows
            foreach (DataRow row in table.Rows)
            {
                var values = columns.Select(c => $"\"{row[c].ToString()?.Replace("\"", "\"\"")}\"");
                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }

        private void PopulateCommitActivitySummary(IEnumerable<IMetric> metrics, ref IndexViewModel model)
        {
            // Process the commit activity data
            var allCommitActivity = metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.COMMIT_COUNT_PER_MONTH)
                .Select(x => x as IResourceMetricValue<XYSeries<string, int>>)
                .Where(x => x != null)
                .Select(x => x!.Value)
                .ToList();

            // Process commit activity data for charts
            if (allCommitActivity.Any())
            {
                // Get all unique months across all repositories
                var allMonths = allCommitActivity
                    .Where(series => series != null)
                    .SelectMany(series => series!.XAxis)
                    .Distinct()
                    .Select(date =>
                    {
                        var parts = date.Split('-');
                        return new
                        {
                            FullDate = date,
                            Year = int.Parse(parts[0]),
                            Month = int.Parse(parts[1])
                        };
                    })
                    .OrderBy(d => d.Year)
                    .ThenBy(d => d.Month)
                    .ToList();

                // Set the sorted months as the CommitMonths

                model.CommitSummary.XAxis = allMonths.Select(d => $"{d.Year}-{d.Month:D2}").ToList();

                var datasets = new List<Series<int>>();

                foreach (var series in allCommitActivity)
                {
                    var errors = series.Validate();
                    if (errors.Count > 0)
                    {
                        // Handle validation errors
                        foreach (var error in errors)
                        {
                            _logger.LogError(error);
                        }
                        continue;
                    }
                    else
                    {
                        foreach (var ySeries in series.YAxis)
                        {
                            var repoName = ySeries.Label;
                            var repoData = new Series<int>
                            {
                                Label = repoName
                            };

                            // For each month in the sorted list, find the corresponding commit count
                            foreach (var month in model.CommitSummary.XAxis)
                            {
                                var index = series.XAxis.IndexOf(month);
                                if (index >= 0 && index < ySeries.Data.Count)
                                {
                                    repoData.Data.Add(ySeries.Data[index]);
                                }
                                else
                                {
                                    // If no data for this month, add 0
                                    repoData.Data.Add(0);
                                }
                            }

                            datasets.Add(repoData);
                        }
                    }
                }
                model.CommitSummary.YAxis = datasets;
            }
        }

        private void PopulateContributors(IEnumerable<IMetric> metrics, ref IndexViewModel model)
        {
            var contributors = metrics.Where(metrics => metrics.Name == MetricName.Repository.Git.CONTRIBUTOR_COMMIT_COUNT)
            .Select(x => x as IResourceMetricValue<Dictionary<string, int>>)
            .Where(x => x != null)
            .Select(x => x!.Value)
            .ToList();

            //consolidate all contributors into one Dictionary<string,int>
            Dictionary<string, int> allContributors = new Dictionary<string, int>();
            foreach (var contributor in contributors)
            {
                foreach (var kvp in contributor)
                {
                    if (allContributors.ContainsKey(kvp.Key))
                    {
                        allContributors[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        allContributors[kvp.Key] = kvp.Value;
                    }
                }
            }
            // Sort contributors by commit count
            model.TopContributors = allContributors.OrderByDescending(x => x.Value).Select(x => (Name: x.Key, CommitCount: x.Value)).ToList();

        }

        private void PopulateTechnologyDistribution(IEnumerable<IMetric> metrics, ref IndexViewModel model)
        {
            var techInfo = metrics.Where(metrics => metrics.Name == MetricName.Repository.FILE_TYPES_BY_EXTENSION)
            .Select(x => x as IResourceMetricValue<DataTable>)
            .Where(x => x != null)
            .Select(x => x!.Value)
            .ToList();

            List<(string Category, string SubCategory, int Count)> technologyDistribution = new List<(string Category, string SubCategory, int Count)>();
            foreach (var tech in techInfo)
            {
                foreach (DataRow row in tech.Rows)
                {
                    string category = Convert.ToString(row[nameof(FileExtensionClassification.Category)]);
                    string subCategory = Convert.ToString(row[nameof(FileExtensionClassification.SubCategory)]);
                    int fileCount = Convert.ToInt32(row[MetricName.Repository.FILE_COUNT]);
                    technologyDistribution.Add((Category: category, SubCategory: subCategory, Count: fileCount));
                }
            }

            // Populate technology distribution. Take the sum of count group by category and subcategory
            model.TechnologyDistribution = technologyDistribution
                .GroupBy(x => new { x.Category, x.SubCategory })
                .Select(g => (Category: g.Key.Category, SubCategory: g.Key.SubCategory, Count: g.Sum(x => x.Count)))
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        private void PopulateTechnologyTable(IEnumerable<IMetric> metrics, ref IndexViewModel model)
        {
            model.TechDataMetricTableName = MetricName.Repository.FILE_TYPES_BY_EXTENSION;
        }
        private void PopulateRepositoryTimeLine(IEnumerable<IMetric> metrics, ref IndexViewModel model)
        {
            model.RepositoryTimelines = new List<RepositoryTimelineData>
                {
                    new RepositoryTimelineData
                    {
                        Name = "CustomerPortal",
                        Color = "#0d6efd",
                        CommitCounts = new List<int> { 124, 145, 165, 198, 210, 245, 268, 312, 356 }
                    },
                    new RepositoryTimelineData
                    {
                        Name = "PaymentProcessor",
                        Color = "#20c997",
                        CommitCounts = new List<int> { 98, 120, 135, 142, 156, 172, 195, 210, 235 }
                    },
                    new RepositoryTimelineData
                    {
                        Name = "AdminDashboard",
                        Color = "#ffc107",
                        CommitCounts = new List<int> { 75, 82, 105, 118, 130, 158, 172, 190, 215 }
                    }
                };
            model.Repositories = new List<RepositoryDetail>
                {
                    new RepositoryDetail
                    {
                        Name = "CustomerPortal",
                        FirstCommitDate = new DateTime(2023, 1, 15),
                        LastCommitDate = DateTime.Now.AddDays(-2),
                        TotalCommits = 1245,
                        ContributorCount = 8,
                        BranchCount = 12,
                        CommitMonths = new List<string> { "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" },
                        CommitCounts = new List<int> { 98, 86, 78, 94, 76, 89 }
                    },
                    new RepositoryDetail
                    {
                        Name = "PaymentProcessor",
                        FirstCommitDate = new DateTime(2023, 2, 10),
                        LastCommitDate = DateTime.Now.AddDays(-5),
                        TotalCommits = 965,
                        ContributorCount = 6,
                        BranchCount = 8,
                        CommitMonths = new List<string> { "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" },
                        CommitCounts = new List<int> { 75, 82, 68, 85, 70, 72 }
                    }
                };
        }



        protected override void LoadData()
        {
            var model = new IndexViewModel();
            var metrics = GetFilteredMetrics();

            PopulateMetricBoxes(metrics, ref model);
            PopulateCommitActivitySummary(metrics, ref model);
            PopulateContributors(metrics, ref model);
            PopulateTechnologyDistribution(metrics, ref model);
            PopulateTechnologyTable(metrics, ref model);
            PopulateRepositoryTimeLine(metrics, ref model);

            Dashboard = model;

        }

    }
}
