using AppSage.Core.Metric;

namespace AppSage.Providers.BasicRepositoryMetric
{
    public static class MetricName
    {
        public static class Repository
        {
            public const string REPOSITORY_COUNT = "AGGREGATE:Repository.RepositoryCount";
            public const string FILE_COUNT = "AGGREGATE:Repository.FileCount";
            public const string LINES_OF_CODE_COUNT = "AGGREGATE:Repository.LinesOfCodeCount";
            public const string REPOSITORY_NAME = "Repository.Name";
            public const string FILE_TYPES_BY_EXTENSION = "Repository.FileTypesByExtensions";
        }

        public static IEnumerable<MetricMetadata> GetMetricMetaData()
        {
            var result = new List<MetricMetadata>();

            result.Add(new MetricMetadata
            {
                Id = Repository.REPOSITORY_COUNT,
                Description = "Total number of repositories found in the workspace repository folder"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.FILE_COUNT,
                Description = "Total number of files in a repository, excluding bin, obj, .git, and .svn folders"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.LINES_OF_CODE_COUNT,
                Description = "Total number of lines of code across all countable files in a repository"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.REPOSITORY_NAME,
                Description = "The name of the repository directory"
            });

            result.Add(new MetricMetadata
            {
                Id = Repository.FILE_TYPES_BY_EXTENSION,
                Description = "Detailed breakdown of file types by extension including category, subcategory, file count, and line count"
            });

            return result;
        }
    }
}
