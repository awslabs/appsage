using LibGit2Sharp;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.ComplexType;
using AppSage.Core.Resource;
using AppSage.Core.Workspace;

namespace AppSage.Providers.Repository
{
    public class GitMetricProvider : IMetricProvider
    {
        // Limit parallel scans to the number of available cores or 4, whichever is smaller
        private int MAX_PARALLEL_SCANS = Math.Min(4, Environment.ProcessorCount);
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        IResourceProvider _gitFolderProvider;
        public GitMetricProvider(IAppSageLogger logger, IAppSageWorkspace workspace)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            _gitFolderProvider = new GitFolderProvider(_logger, _workspace);
        }

        public string FullQualifiedName => GetType().FullName;

        public string Description => "Provide Git based repository statistics and information.";

        public void Run(IMetricCollector metrics)
        {

            try
            {
                _logger.LogInformation($"Running the provider:{GetType().FullName}");

                var gitFolderList = _gitFolderProvider.GetResources();
                metrics.Add(new MetricValue<int>
                 (
                     name: MetricName.Repository.Git.REPOSITORY_COUNT,
                     provider: this.FullQualifiedName,
                     value: gitFolderList.Count()
                 ));

                Dictionary<string, int> totalContributorCommits = new Dictionary<string, int>();

                gitFolderList.AsParallel().WithDegreeOfParallelism(MAX_PARALLEL_SCANS).ForAll(resource =>
                {
                    string folder = resource.Path;
                    _logger.LogInformation($"Processing:{folder}");
                    try
                    {
                        _logger.LogInformation($"Processing git repository: {folder}");

                        // Fully qualify the Repository class to avoid ambiguity
                        using (var repo = new LibGit2Sharp.Repository(folder))
                        {
                            // Get repository name
                            string repoName = new DirectoryInfo(folder).Name;

                            // Number of branches
                            int branchCount = repo.Branches.Count();
                            metrics.Add(new ResourceMetricValue<int>
                            (
                                name: MetricName.Repository.Git.BRANCH_COUNT,
                                provider: this.FullQualifiedName,
                                segment: repoName,
                                resource: resource.Name,
                                value: branchCount
                            ));

                            // Number of contributors
                            var contributors = repo.Commits.Select(c => c.Author.Name).Distinct();
                            metrics.Add(new ResourceMetricValue<int>
                            (
                                name: MetricName.Repository.Git.CONTRIBUTOR_COUNT,
                                provider: this.FullQualifiedName,
                                segment: repoName,
                                resource: resource.Name,
                                value: contributors.Count()
                            ));

                            Dictionary<string, int> contributorCommits = new Dictionary<string, int>();

                            //Number of commits by each contributor
                            foreach (var contributor in contributors)
                            {
                                var commitCount = repo.Commits.Count(c => c.Author.Name == contributor);
                                contributorCommits[contributor] = commitCount;
                                totalContributorCommits[contributor] = totalContributorCommits.ContainsKey(contributor) ? totalContributorCommits[contributor] + commitCount : commitCount;
                            }

                            metrics.Add(new ResourceMetricValue<Dictionary<string, int>>
                            (
                                name: MetricName.Repository.Git.CONTRIBUTOR_COMMIT_COUNT,
                                provider: this.FullQualifiedName,
                                segment: repoName,
                                resource: resource.Name,
                                value: contributorCommits
                            ));

                            // Get all commits
                            var commits = repo.Commits.ToList();

                            if (commits.Any())
                            {
                                // Last commit date
                                var lastCommit = commits.First();

                                metrics.Add(new ResourceMetricValue<DateTimeOffset>
                                (
                                      name: MetricName.Repository.Git.LAST_COMMIT_DATE,
                                      provider: this.FullQualifiedName,
                                      segment: repoName,
                                      resource: resource.Name,
                                      value: lastCommit.Author.When
                                ));

                                // First commit date (oldest commit)
                                var firstCommit = commits.Last();

                                metrics.Add(new ResourceMetricValue<DateTimeOffset>
                                (
                                name: MetricName.Repository.Git.FIRST_COMMIT_DATE,
                                provider: this.FullQualifiedName,
                                segment: repoName,
                                resource: resource.Name,
                                value: firstCommit.Author.When
                                ));

                                // Total number of commits

                                metrics.Add(new ResourceMetricValue<int>
                                (
                                    name: MetricName.Repository.Git.TOTAL_COMMIT_COUNT,
                                    provider: this.FullQualifiedName,
                                    segment: repoName,
                                    resource: resource.Name,
                                    value: commits.Count
                                ));

                                // Number of commits by month
                                var commitsByMonth = commits
                                    .GroupBy(c => new { c.Author.When.Year, c.Author.When.Month })
                                    .Select(g => new
                                    {
                                        g.Key.Year,
                                        g.Key.Month,
                                        Count = g.Count()
                                    })
                                    .OrderBy(x => x.Year)
                                    .ThenBy(x => x.Month)
                                    .ToList();

                                XYSeries<string, int> commitData = new XYSeries<string, int>();
                                commitData.XAxis.AddRange(commitsByMonth.Select(item => $"{item.Year}-{item.Month}"));

                                string shortName = new DirectoryInfo(resource.Name).Name;
                                commitData.YAxis.Add(new Series<int>(label: shortName, data: commitsByMonth.Select(x => x.Count).ToList()));

                                metrics.Add(new ResourceMetricValue<XYSeries<string, int>>
                                (
                                    name: MetricName.Repository.Git.COMMIT_COUNT_PER_MONTH,
                                    provider: this.FullQualifiedName,
                                    segment: repoName,
                                    resource: resource.Name,
                                    value: commitData
                                ));

                            }
                        }

                        metrics.Add(new MetricValue<Dictionary<string, int>>
                        (
                            name: MetricName.Repository.Git.TOTAL_CONTRIBUTOR_COMMIT_COUNT,
                            provider: this.FullQualifiedName,
                            value: totalContributorCommits
                        ));

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing git repository at {folder}: {ex.Message}");
                    }

                });
                _logger.LogInformation($"{FullQualifiedName}:[Completed]");
            }
            finally
            {
                metrics.CompleteAdding();
            }

        }
    }


}
