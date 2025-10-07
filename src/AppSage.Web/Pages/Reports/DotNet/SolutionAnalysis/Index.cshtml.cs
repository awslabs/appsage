using Amazon.Runtime;
using AppSage.Core.Configuration;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Web.Components.Filter;
using ClosedXML.Excel;
using System.Data;

namespace AppSage.Web.Pages.Reports.DotNet.SolutionAnalysis
{
    public enum DataExportName
    {
        ProjectNugetReferences,
        ProjectReferenceUsageApproximation,
        ProjectClassReferenceUsageApproximation,
        LibraryImpactAnalysis,
        LibraryImpactAnalysisExpanded,
        ClassStatistics,
        MethodStatistics
    }
    public class IndexModel : MetricFilterPageModel
    {
        public IndexModel(IMetricReader metricReader) : base(metricReader) { }
        public IndexViewModel Dashboard { get; set; } = new IndexViewModel();

        public override List<IMetric> GetMyMetrics()
        {
            //in this report we are using only those stats reported by AppSage.Providers.DotNet.DotNetBasicCodeAnalysisProvider
            string providerName = "AppSage.Providers.DotNet.BasicCodeAnalysis.DotNetBasicCodeAnalysisProvider";

            var allMetrics=GetAllMetrics();



            var result=allMetrics.Where(x => x.Provider == providerName).ToList();
            return result;
        }

        protected override IEnumerable<DataTable> ExportData(string dataExportName)
        {
            if(Enum.TryParse(dataExportName, out DataExportName exportType))
            {
                var metrics = GetAllMetrics();
                var allTables = metrics.Where(m => m is IResourceMetricValue<DataTable>).Select(m => m as IResourceMetricValue<DataTable>);

                IEnumerable<DataTable> metricSet = null;

                switch (exportType)
                {
                    case DataExportName.ProjectNugetReferences:
                        {
                            metricSet = allTables.Where(m => m.Name==MetricName.DotNet.Project.NUGET_PACKAGES && m.Value.Rows.Count>0).Select(m => m.Value).ToList();
                            break;
                        }
                    case DataExportName.ProjectReferenceUsageApproximation:
                        {
                            metricSet = allTables.Where(m => m.Name == MetricName.DotNet.Project.PROJECT_REFERENCE_USAGE_APROXIMATION && m.Value.Rows.Count > 0).Select(m => m.Value).ToList();
                            break;
                        }
                    case DataExportName.ProjectClassReferenceUsageApproximation:
                        {
                            metricSet = allTables.Where(m => m.Name == MetricName.DotNet.Project.CLASS_REFERENCE_USAGE_APROXIMATION && m.Value.Rows.Count > 0).Select(m => m.Value).ToList();
                            break; 
                        }

                    case DataExportName.LibraryImpactAnalysis:
                        {
                            metricSet = allTables.Where(m => m.Name == MetricName.DotNet.Project.PROJECT_LIBRARY_IMPACT_APPROXIMATION && m.Value.Rows.Count > 0).Select(m => m.Value).ToList();
                            break;

                        }
                    case DataExportName.LibraryImpactAnalysisExpanded:
                        {
                            metricSet = allTables.Where(m => m.Name == MetricName.DotNet.Project.PROJECT_LIBRARY_IMPACT_APPROXIMATION_EXPANDED && m.Value.Rows.Count > 0).Select(m => m.Value).ToList();
                            break;
                        }
                    case DataExportName.ClassStatistics:
                        {
                            metricSet = allTables.Where(m => m.Name == MetricName.DotNet.Project.CLASS_STATISTICS && m.Value.Rows.Count > 0).Select(m => m.Value).ToList();
                            break;
                        }
                    case DataExportName.MethodStatistics:
                        {
                            metricSet = allTables.Where(m => m.Name == MetricName.DotNet.Project.METHOD_STATISTICS && m.Value.Rows.Count > 0).Select(m => m.Value).ToList();
                            break;
                        }
                }

                DataTable tableToBeExported = new DataTable();

                if (metricSet!=null & metricSet.Any())
                {
                    var table = metricSet.FirstOrDefault();
                    var currentTable = table.Clone();
                    currentTable.TableName = Enum.GetName(typeof(DataExportName), exportType);

                    metricSet.ToList().ForEach(m =>
                    {

                        foreach (DataRow row in m.Rows)
                        {
                            currentTable.ImportRow(row);
                        }

                    });
                    tableToBeExported = currentTable;
                }

                return new List<DataTable> { tableToBeExported };

            }
            else
            {
                return null;
            }
        }

       

        protected override void LoadData()
        {
            var metrics = GetFilteredMetrics();
            // FilterPossibleValue metrics that have a Resource property (implementing IResourceMetricValue interface)
            var resourceMetrics = metrics.Where(m => m.GetType().GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResourceMetricValue<>)))
                .ToList();
            var projectGroups = resourceMetrics.GroupBy(m => GetResourceFromMetric(m)).ToList();

            Dashboard.Projects = new List<ProjectMetrics>();

            var nugetInfo = new Dictionary<string, (int NumberOfProjects, HashSet<string> Versions)>();

            foreach (var projectGroup in projectGroups)
            {
                var projectMetrics = new ProjectMetrics
                {
                    ProjectPath = projectGroup.Key
                };

                // Extract project name from path
                projectMetrics.ProjectName = Path.GetFileNameWithoutExtension(projectGroup.Key);

                foreach (var metric in projectGroup)
                {
                    projectMetrics.Segment = metric.Segment;

                    switch (metric.Name)
                    {
                        case MetricName.DotNet.Project.LANGUAGE:
                            if (metric is IResourceMetricValue<string> stringMetric)
                                projectMetrics.Language = stringMetric.Value ?? string.Empty;
                            break;
                        case MetricName.DotNet.Project.TYPE:
                            if (metric is IResourceMetricValue<string> typeMetric)
                                projectMetrics.ProjectType = typeMetric.Value ?? string.Empty;
                            break;
                        case MetricName.DotNet.Project.DOTNET_VERSION:
                            if (metric is IResourceMetricValue<string> versionMetric)
                                projectMetrics.DotNetVersion = versionMetric.Value ?? string.Empty;
                            break;
                        case MetricName.DotNet.Project.DOCUMENT_COUNT_TOTAL:
                            if (metric is IResourceMetricValue<int> docMetric)
                                projectMetrics.DocumentCount = docMetric.Value;
                            break;
                        case MetricName.DotNet.Project.CLASS_COUNT:
                            if (metric is IResourceMetricValue<int> classMetric)
                                projectMetrics.ClassCount = classMetric.Value;
                            break;
                        case MetricName.DotNet.Project.METHOD_COUNT:
                            if (metric is IResourceMetricValue<int> methodMetric)
                                projectMetrics.MethodCount = methodMetric.Value;
                            break;
                        case MetricName.DotNet.Project.DATA_CLASS_COUNT:
                            if (metric is IResourceMetricValue<int> dataClassMetric)
                                projectMetrics.DataClassCount = dataClassMetric.Value;
                            break;
                        case MetricName.DotNet.Project.NUGET_PACKAGES:
                            {
                                if (metric is IResourceMetricValue<DataTable> packageMetric)
                                {
                                    projectMetrics.NugetPackages = packageMetric.Value;
                                    foreach(DataRow row in packageMetric.Value.Rows)
                                    {
                                        string packageNameColumn=AppSage.Providers.DotNet.ConstString.ExternalReference.Attributes.ReferenceName;
                                        string versionColumn= AppSage.Providers.DotNet.ConstString.ExternalReference.Attributes.ReferenceVersion;
                                        

                                        var packageName = row[packageNameColumn]?.ToString() ?? string.Empty;
                                        var version = row[versionColumn]?.ToString() ?? string.Empty;
                                        if (!string.IsNullOrEmpty(packageName))
                                        {
                                            if (!nugetInfo.ContainsKey(packageName))
                                            {
                                                nugetInfo[packageName] = (0, new HashSet<string>());
                                            }
                                            var dataSet=nugetInfo[packageName];
                                            dataSet.Versions.Add(version);
                                            dataSet.NumberOfProjects++;
                                            nugetInfo[packageName] = dataSet;
                                        }
                                    }

                                }
                                break;
                            }
                    }
                }
                Dashboard.Projects.Add(projectMetrics);
            }

            Dashboard.ReferencesMetricTableName = MetricName.DotNet.Project.NUGET_PACKAGES;


            Dashboard.DataExports = new List<(string Title, string Description, DataExportName)>();
            foreach (DataExportName exportName in Enum.GetValues(typeof(DataExportName)))
            {
                string title = string.Empty;
                string description = string.Empty;  
                switch (exportName)
                {
                    case DataExportName.ProjectNugetReferences:
                        {
                            title = "NuGet References";
                            description = "Export NuGet package references for each project";
                            break;
                        }
                    case DataExportName.ProjectReferenceUsageApproximation:
                        {
                            title = "Project ClassReferences Usage Approximation";
                            description = "The namespaces used in a .NET related documents in the project. This can be use to approximate the impact of a libraries in the project";
                            break;
                        }
                    case DataExportName.ProjectClassReferenceUsageApproximation:
                        {
                            title = "Project Class ClassReferences Usage Approximation";
                            description = "The name spaces used in classes in .NET related documents in the project. This can be use to approximate the impact of a libraries in classes";
                            break;
                        }
                    case DataExportName.LibraryImpactAnalysis:
                        {
                            title = "Library Impact Analysis : Short";
                            description = "The impact of libraries in the project. This is an approximation based on the namespaces used in the project.";
                            break;
                        }
                    case DataExportName.LibraryImpactAnalysisExpanded:
                        {
                            title = "Library Impact Analysis : Expanded";
                            description = "The impact of libraries in the project. This is an approximation based on the namespaces used in the project. It includes detailed information about the classes and methods used from each library.";
                            break;  
                        }
                    case DataExportName.ClassStatistics:
                        {
                            title = "How classes uses certain namespaces : Expanded";
                            description = "The impact of libraries in the classes. This is an approximation based on the namespaces used in the classes. It includes detailed information about the methods and references used in classes.";
                            break;
                        }
                    case DataExportName.MethodStatistics:
                        {
                            title = "Method related statistics : Expanded";
                            description = "You get the relationships between projects, classes, methods, namespaces";
                            break;
                        }
                }
                Dashboard.DataExports.Add((title, description, exportName));
            }

            // Calculate summary statistics
            CalculateSummary();
        }

        private void CalculateSummary()
        {
            Dashboard.Summary = new SolutionSummary
            {
                TotalProjects = Dashboard.Projects.Count,
                TotalClasses = Dashboard.Projects.Sum(p => p.ClassCount),
                TotalMethods = Dashboard.Projects.Sum(p => p.MethodCount),
                TotalDocuments = Dashboard.Projects.Sum(p => p.DocumentCount),
                TotalDataClasses = Dashboard.Projects.Sum(p => p.DataClassCount)
            };

            // Language distribution
            Dashboard.Summary.LanguageDistribution = Dashboard.Projects
                .GroupBy(p => p.Language)
                .ToDictionary(g => g.Key, g => g.Count());

            // Project type distribution
            Dashboard.Summary.ProjectTypeDistribution = Dashboard.Projects
                .GroupBy(p => p.ProjectType)
                .ToDictionary(g => g.Key, g => g.Count());

            // .NET version distribution
            Dashboard.Summary.DotNetVersionDistribution = Dashboard.Projects
                .GroupBy(p => p.DotNetVersion)
                .ToDictionary(g => g.Key, g => g.Count());

            // Top NuGet packages
            var packageCounts = new Dictionary<string, int>();
            foreach (var project in Dashboard.Projects)
            {
                if (project.NugetPackages != null)
                {
                    foreach (DataRow row in project.NugetPackages.Rows)
                    {
                        var packageName = row["ReferenceName"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(packageName))
                        {
                            packageCounts[packageName] = packageCounts.GetValueOrDefault(packageName, 0) + 1;
                        }
                    }
                }
            }

            Dashboard.Summary.TopNugetPackages = packageCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
        }

        private string GetResourceFromMetric(IMetric metric)
        {
            // Use reflection to get the Resource property from any IResourceMetricValue<T>
            var resourceProperty = metric.GetType().GetProperty("Resource");
            return resourceProperty?.GetValue(metric)?.ToString() ?? string.Empty;
        }


        private XLWorkbook ConverToWorkBook(DataTable table)
        {
            string worksheetName = "Sheet1";
            if (!string.IsNullOrEmpty(table.TableName))
            {
                if (table.TableName.Length > 31)
                {
                    // Excel worksheet names cannot exceed 31 characters
                    worksheetName = table.TableName.Substring(0, 31);
                }
                else
                {
                    worksheetName = table.TableName;
                }
            }
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(worksheetName);
            worksheet.Cell(1, 1).InsertTable(table);
            return workbook;
        }
    }
}
