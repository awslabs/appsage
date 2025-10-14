using AppSage.Core.Metric;


namespace AppSage.Providers.GitMetric
{
    public static class MetricName
    {
        public static class Repository
        {

            public static class Git
            {
                public const string REPOSITORY_COUNT = "Repository.Git.RepositoryCount";
                public const string BRANCH_COUNT = "Repository.Git.BranchCount";
                public const string LAST_COMMIT_DATE = "Repository.Git.LastCommitDate";
                public const string FIRST_COMMIT_DATE = "Repository.Git.FirstCommitDate";
                public const string TOTAL_COMMIT_COUNT = "Repository.Git.CommitCount";
                public const string COMMIT_COUNT_PER_MONTH = "Repository.Git.CommitCountPerMonth";
                public const string TOTAL_CONTRIBUTOR_COMMIT_COUNT = "Repository.Git.TotalContributorCount";
                public const string CONTRIBUTOR_COUNT = "Repository.Git.ContributorCount";
                public const string CONTRIBUTOR_COMMIT_COUNT = "Repository.Git.ContributorCommitCount";
            }




        }

        public static IEnumerable<MetricMetadata> GetMetricMetaData()
        {
            var result = new List<MetricMetadata>();

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.REPOSITORY_COUNT,
                Description = "Total number of Git repositories found in the workspace"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.BRANCH_COUNT,
                Description = "Number of branches in a Git repository"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.LAST_COMMIT_DATE,
                Description = "Date and time of the most recent commit in a Git repository"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.FIRST_COMMIT_DATE,
                Description = "Date and time of the oldest commit in a Git repository"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.TOTAL_COMMIT_COUNT,
                Description = "Total number of commits in a Git repository"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.COMMIT_COUNT_PER_MONTH,
                Description = "Number of commits grouped by month and year as a time series"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.TOTAL_CONTRIBUTOR_COMMIT_COUNT,
                Description = "Aggregate count of commits per contributor across all Git repositories"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.CONTRIBUTOR_COUNT,
                Description = "Number of unique contributors in a Git repository"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.Git.CONTRIBUTOR_COMMIT_COUNT,
                Description = "Number of commits per contributor in a specific Git repository"
            });

            return result;

        }
    }
}
