using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using System.Data;

namespace AppSage.Providers.BasicRepositoryMetric
{
    public class RepositoryMetricProvider : IMetricProvider,IMetricMetadataProvider
    {
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        private int MAX_PARALLEL_SCANS = Math.Min(4, Environment.ProcessorCount);
        public string FullQualifiedName => this.GetType().FullName;

        public string Description => "Provides basic repository information. It identifies content based on file types";

        public IEnumerable<MetricMetadata> MetricInfo => MetricName.GetMetricMetaData();

        private Dictionary<string, FileExtensionClassification> _fileExtensionClassification = FileExtensionClassification.GetComprehensiveFileTypes();
        public RepositoryMetricProvider(IAppSageLogger logger, IAppSageWorkspace workspace)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        }

        public void Run(IMetricCollector metrics)
        {
            try
            {
                var repositoryList = Directory.GetDirectories(_workspace.RepositoryFolder).ToList();

                metrics.Add(new MetricValue<int>
                (
                    name: MetricName.Repository.REPOSITORY_COUNT,
                    provider: this.FullQualifiedName,
                    value: repositoryList.Count
                ));


                repositoryList.AsParallel().WithDegreeOfParallelism(MAX_PARALLEL_SCANS).ForAll(repo =>
                {
                    string resourceName = _workspace.GetResourceName(repo);

                    _logger.LogInformation("Processing repository: {ResourceName}", resourceName);

                    metrics.Add(new ResourceMetricValue<string>
                     (
                         name: MetricName.Repository.REPOSITORY_NAME,
                         provider: this.FullQualifiedName,
                         segment: resourceName,
                         resource: resourceName,
                         value: new DirectoryInfo(repo).Name
                     ));

                    Stack<string> foldersToProcess = new();
                    foldersToProcess.Push(repo);


                    Dictionary<string, (int FileCount, int LineCount)> fileCountLineCountByExtension = new();
                    while (foldersToProcess.Count > 0)
                    {
                        string currentFolder = foldersToProcess.Pop();


                        try
                        {
                            // Count files in current directory
                            var files = Directory.GetFiles(currentFolder);


                            foreach (var file in files)
                            {
                                string extension = Path.GetExtension(file);
                                if (_fileExtensionClassification.ContainsKey(extension))
                                {
                                    //ensure the extension is in the dictionary
                                    fileCountLineCountByExtension.TryAdd(extension, (0, 0));

                                    var currentCounts = fileCountLineCountByExtension[extension];
                                    currentCounts.FileCount += 1;
                                    if (_fileExtensionClassification[extension].LineCountable)
                                    {
                                        // Count lines in the file
                                        int lineCount = File.ReadAllLines(file).Length;
                                        currentCounts.LineCount += lineCount;
                                    }
                                    fileCountLineCountByExtension[extension] = currentCounts;
                                }
                                else
                                {
                                    //ensure the extension is in the dictionary
                                    fileCountLineCountByExtension.TryAdd(FileExtensionClassification.UNKOWN, (0, 0));
                                    var currentCounts = fileCountLineCountByExtension[FileExtensionClassification.UNKOWN];
                                    currentCounts.FileCount += 1;
                                    fileCountLineCountByExtension[FileExtensionClassification.UNKOWN] = currentCounts;
                                }

                            }


                            // Add subdirectories to the stack
                            foreach (string subDir in Directory.GetDirectories(currentFolder))
                            {
                                // Ignore bin, obj, .git, and .svn folders
                                var folderName = Path.GetFileName(subDir).ToLowerInvariant();
                                if (folderName == "bin" || folderName == "obj" || folderName == ".git" || folderName == ".svn")
                                {
                                    continue; // Skip bin, obj, .git, and .svn folders
                                }

                                foldersToProcess.Push(subDir);
                            }
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            _logger.LogWarning("Access denied to {CurrentFolder}: {Message}", currentFolder, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Error accessing {CurrentFolder}: {Message}", currentFolder, ex.Message);
                        }
                    }

                    metrics.Add(new ResourceMetricValue<int>
                    (
                     name: MetricName.Repository.FILE_COUNT,
                     provider: this.FullQualifiedName,
                     segment: resourceName,
                     resource: resourceName,
                     value: fileCountLineCountByExtension.Sum(c => c.Value.FileCount)
                    ));

                    metrics.Add(new ResourceMetricValue<int>
                    (
                    name: MetricName.Repository.LINES_OF_CODE_COUNT,
                    provider: this.FullQualifiedName,
                    segment: resourceName,
                    resource: resourceName,
                    value: fileCountLineCountByExtension.Sum(c => c.Value.LineCount)
                    ));

                    DataTable fileTypeSummary = new DataTable();
                    fileTypeSummary.Columns.Add(nameof(FileExtensionClassification.Category), typeof(string));
                    fileTypeSummary.Columns.Add(nameof(FileExtensionClassification.SubCategory), typeof(string));
                    fileTypeSummary.Columns.Add(MetricName.Repository.FILE_COUNT, typeof(int));
                    fileTypeSummary.Columns.Add(MetricName.Repository.LINES_OF_CODE_COUNT, typeof(int));


                    foreach (var fileType in fileCountLineCountByExtension)
                    {
                        var fileTypeClassification = _fileExtensionClassification[fileType.Key];
                        DataRow row = fileTypeSummary.NewRow();
                        row[nameof(FileExtensionClassification.Category)] = fileTypeClassification.Category;
                        row[nameof(FileExtensionClassification.SubCategory)] = fileTypeClassification.SubCategory;
                        row[MetricName.Repository.FILE_COUNT] = fileType.Value.FileCount;
                        row[MetricName.Repository.LINES_OF_CODE_COUNT] = fileType.Value.LineCount;
                        fileTypeSummary.Rows.Add(row);
                    }
                    IResourceMetricValue<DataTable> repoSummary = new ResourceMetricValue<DataTable>
                    (
                        name: MetricName.Repository.FILE_TYPES_BY_EXTENSION,
                        provider: this.FullQualifiedName,
                        segment: resourceName,
                        resource: resourceName,
                        value: fileTypeSummary
                    );
                    metrics.Add(repoSummary);
                    _logger.LogInformation("Finished processing repository: {ResourceName}", resourceName);


                });
                _logger.LogInformation("{FullQualifiedName}:[Completed]", FullQualifiedName);
            }
            finally
            {
                // Ensure all metrics are flushed
                metrics.CompleteAdding();
            }
        }
    }
}

