using AppSage.Core.Configuration;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Resource;
using AppSage.Core.Workspace;
using AppSage.Providers.DotNet.Utility;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Data;

namespace AppSage.Providers.DotNet.BasicCodeAnalysis
{
    public class DotNetBasicCodeAnalysisProvider : IMetricProvider
    {
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        ProjectAnalyzer _projectAnalyzer;
        
        object _padlock = new object();

        private (int DocumentMaxParallelism, int ProjectMaxParallelism, string NamespaceListFileToIdentifyDBRelatedClasses, string MSBuildPath) _config;

        public DotNetBasicCodeAnalysisProvider(IAppSageLogger logger, IAppSageWorkspace workspace, IAppSageConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectAnalyzer = new ProjectAnalyzer(_logger,configuration);
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
           

            _config.MSBuildPath = configuration.Get<string>("AppSage.Providers.DotNet.SHARED:MSBuildPath");
            _config.DocumentMaxParallelism = configuration.Get<int>("AppSage.Providers.DotNet.BasicCodeAnalysis.DotNetBasicCodeAnalysisProvider:DocumentMaxParallelism");
            _config.ProjectMaxParallelism = configuration.Get<int>("AppSage.Providers.DotNet.BasicCodeAnalysis.DotNetBasicCodeAnalysisProvider:ProjectMaxParallelism");
            _config.NamespaceListFileToIdentifyDBRelatedClasses = configuration.Get<string>("AppSage.Providers.DotNet.BasicCodeAnalysis.DotNetBasicCodeAnalysisProvider:NamespaceListFileToIdentifyDBRelatedClasses");
        }

        public string FullQualifiedName => GetType().FullName;

        public string Description => "Provides basic .NET code analysis";

        public void Run(IMetricCollector metrics)
        {
            try
            {

                if (!MSBuildLocator.IsRegistered)
                {
                    MSBuildLocator.RegisterMSBuildPath(_config.MSBuildPath);
                }

                //Register MSBuild instance
                //MSBuildLocator.RegisterDefaults();
                IResourceProvider projectFileProvider = new ProjectFileProvider(_logger, _workspace);
                var projectFileList = projectFileProvider.GetResources().ToList();

                int progress = 0;

                projectFileList.AsParallel().WithDegreeOfParallelism(_config.ProjectMaxParallelism).ForAll(projectFile =>
                {
                    lock (_padlock)
                    {
                        progress++;
                        _logger.LogInformation("Processing project file: [{Progress}/{Total}] : {ProjectName}", progress, projectFileList.Count(), projectFile.Name);
                    }

                    try
                    {
                        using (var workspace = MSBuildWorkspace.Create())
                        {

                            var openJob = workspace.OpenProjectAsync(projectFile.Path).ContinueWith(task =>
                            {
                                if (task.IsCompletedSuccessfully)
                                {
                                    Project project = task.Result;

                                    var basicInfo = GetBasicMetrics(project);
                                    basicInfo.ForEach(m => { metrics.Add(m); });

                                    var referenceList = LibraryImpactSummary(project);
                                    referenceList.ForEach(m => { metrics.Add(m); });

                                }
                                else
                                {
                                    _logger.LogError("Failed to open project {ProjectName}", projectFile.Name, task.Exception);
                                }
                            });
                            openJob.Wait();
                            _logger.LogInformation("Processed project file: {ProjectName}", projectFile.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error processing project file {ProjectName}: {ErrorMessage}", projectFile.Name, ex.Message, ex);
                    }
                });
                _logger.LogInformation("{FullQualifiedName}:Project Analysis:[Completed]", FullQualifiedName);

            }
            finally
            {
                metrics.CompleteAdding();
            }
        }

        private List<IMetric> GetBasicMetrics(Project p)
        {
            List<IMetric> metrics = new List<IMetric>();

            string repoName = _workspace.GetRepositoryName(p.FilePath);
            string assemblyName = p.AssemblyName ?? ConstString.UNKNOWN;
            string segmentIdentifier = $"/{repoName}/{assemblyName}";
            string resourceName = _workspace.GetResourceName(p.FilePath);


            metrics.Add(new ResourceMetricValue<string>
            (
                name: MetricName.DotNet.Project.LANGUAGE,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: p.Language
            ));
            metrics.Add(new ResourceMetricValue<string>
            (
                name: MetricName.DotNet.Project.TYPE,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: _projectAnalyzer.GetProjectType(p)
            ));

            metrics.Add(new ResourceMetricValue<string>
            (
                name: MetricName.DotNet.Project.DOTNET_VERSION,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: _projectAnalyzer.ParseDotNetVersion(p.FilePath)
            ));

            metrics.Add(new ResourceMetricValue<int>
            (
                name: MetricName.DotNet.Project.DOCUMENT_COUNT_TOTAL,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: p.Documents.Count()

            ));
            metrics.Add(new ResourceMetricValue<int>
            (
                name: MetricName.DotNet.Project.DOCUMENT_COUNT_PUREDOTNET,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: p.Documents.Count(d => d.SourceCodeKind == SourceCodeKind.Regular)
            ));
            metrics.Add(new ResourceMetricValue<int>
            (
                name: MetricName.DotNet.Project.DOCUMENT_COUNT_SCRIPTS,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: p.Documents.Count(d => d.SourceCodeKind == SourceCodeKind.Script)
            ));

            DataTable nugetReferences = new DataTable();

            nugetReferences.Columns.Add(ConstString.ExternalReference.Attributes.ProjectPath, typeof(string));
            nugetReferences.Columns.Add(ConstString.ExternalReference.Attributes.ProjectAssemblyName, typeof(string));
            nugetReferences.Columns.Add(ConstString.ExternalReference.Attributes.ReferenceName, typeof(string));
            nugetReferences.Columns.Add(ConstString.ExternalReference.Attributes.ReferenceVersion, typeof(string));
            nugetReferences.Columns.Add(ConstString.ExternalReference.Attributes.ReferenceCategory, typeof(string)); // Framework or Other
            nugetReferences.Columns.Add(ConstString.ExternalReference.Attributes.ReferenceType, typeof(string)); //How the package is referenced. E.g. as a project, as a Nuget package or as a DLL

            foreach (var reference in _projectAnalyzer.GetReferences(p.FilePath))
            {
                nugetReferences.Rows.Add(resourceName, p.AssemblyName, reference.Name, reference.Version, reference.Category, reference.ReferenceType);
            }


            metrics.Add(new ResourceMetricValue<DataTable>
            (
                name: MetricName.DotNet.Project.NUGET_PACKAGES,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: nugetReferences
            ));


            var aggregateSummary = _projectAnalyzer.GetDocumentSummary(p.Documents);


            metrics.Add(new ResourceMetricValue<int>
            (
                name: MetricName.DotNet.Project.CLASS_COUNT,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: aggregateSummary.NameTypeCount
            ));
            metrics.Add(new ResourceMetricValue<int>
           (
               name: MetricName.DotNet.Project.METHOD_COUNT,
               segment: segmentIdentifier,
               resource: resourceName,
               provider: FullQualifiedName,
               value: aggregateSummary.MethodCount
           ));
            metrics.Add(new ResourceMetricValue<int>
           (
               name: MetricName.DotNet.Project.DATA_CLASS_COUNT,
               segment: segmentIdentifier,
               resource: resourceName,
               provider: FullQualifiedName,
               value: aggregateSummary.DataOnlyNameTypeCount
           ));

            metrics.Add(new ResourceMetricValue<int>
            (
               name: MetricName.DotNet.Project.LINES_OF_CODE,
               segment: segmentIdentifier,
               resource: resourceName,
               provider: FullQualifiedName,
               value: aggregateSummary.LinesOfCode
            ));


            DataTable classStats = new DataTable();
            classStats.Columns.Add(ConstString.ClassInfo.ProjectPath, typeof(string));
            classStats.Columns.Add(ConstString.ClassInfo.ProjectAssemblyName, typeof(string));
            classStats.Columns.Add(ConstString.ClassInfo.ClassName, typeof(string));
            classStats.Columns.Add(ConstString.ClassInfo.ClassMethodCount, typeof(int));
            classStats.Columns.Add(ConstString.ClassInfo.ClassLinesOfCode, typeof(int));
            classStats.Columns.Add(ConstString.ClassInfo.IsClassAffectedByDB, typeof(bool));
            classStats.Columns.Add(ConstString.ClassInfo.ClassReferenceNamespaces, typeof(string));


            DataTable methodStats = new DataTable();
            methodStats.Columns.Add(ConstString.MethodInfo.ProjectPath, typeof(string));
            methodStats.Columns.Add(ConstString.MethodInfo.ProjectAssemblyName, typeof(string));
            methodStats.Columns.Add(ConstString.MethodInfo.ClassName, typeof(string));
            methodStats.Columns.Add(ConstString.MethodInfo.ClassMethodCount, typeof(int));
            methodStats.Columns.Add(ConstString.MethodInfo.ClassLinesOfCode, typeof(int));
            methodStats.Columns.Add(ConstString.MethodInfo.IsClassAffectedByDB, typeof(bool));
            methodStats.Columns.Add(ConstString.MethodInfo.ClassReferenceNamespaces, typeof(string));
            methodStats.Columns.Add(ConstString.MethodInfo.MethodName, typeof(string));
            methodStats.Columns.Add(ConstString.MethodInfo.MethodLinesOfCode, typeof(int));
            methodStats.Columns.Add(ConstString.MethodInfo.MethodMaxParameters, typeof(int));

            HashSet<string> dbRelatedNamespaces = GetDBRelatedNamespaces();

            foreach (var classEntry in aggregateSummary.NameTypeInfo)
            {
                bool isClassAffectedByDB = classEntry.Value.References.Any(r => dbRelatedNamespaces.Contains(r));

                string refenceList = string.Join("\r\n", classEntry.Value.References);
                classStats.Rows.Add(resourceName, assemblyName, classEntry.Key, classEntry.Value.MethodCount, classEntry.Value.LinesOfCode, isClassAffectedByDB, refenceList);

                foreach(var methodEntry in classEntry.Value.MethodInfo)
                {
                   
                    methodStats.Rows.Add(resourceName, assemblyName, classEntry.Key, classEntry.Value.MethodCount, classEntry.Value.LinesOfCode, isClassAffectedByDB, refenceList,
                        methodEntry.Key, methodEntry.Value.LinesOfCode, methodEntry.Value.NumberOfParameters);
                }
            }

            metrics.Add(new ResourceMetricValue<DataTable>
            (
            name: MetricName.DotNet.Project.CLASS_STATISTICS,
            segment: segmentIdentifier,
            resource: resourceName,
            provider: FullQualifiedName,
            value: classStats
            ));

            metrics.Add(new ResourceMetricValue<DataTable>
            (
            name: MetricName.DotNet.Project.METHOD_STATISTICS,
            segment: segmentIdentifier,
            resource: resourceName,
            provider: FullQualifiedName,
            value: methodStats
            ));

            DataTable projectReferenceUsage = new DataTable();
            projectReferenceUsage.Columns.Add(ConstString.ProjectInfo.ProjectPath, typeof(string));
            projectReferenceUsage.Columns.Add(ConstString.ProjectInfo.ProjectAssemblyName, typeof(string));
            projectReferenceUsage.Columns.Add(ConstString.ProjectInfo.ReferenceUsage, typeof(string));

            aggregateSummary.NameTypeInfo.Values.SelectMany(c => c.References).Distinct().ToList().ForEach(reference =>
            {
                projectReferenceUsage.Rows.Add(resourceName, assemblyName, reference);
            });


            metrics.Add(new ResourceMetricValue<DataTable>
            (
                name: MetricName.DotNet.Project.PROJECT_REFERENCE_USAGE_APROXIMATION,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: projectReferenceUsage
            ));

            return metrics;
        }

        private List<IMetric> LibraryImpactSummary(Project p)
        {
            List<IMetric> result = new List<IMetric>();

            var projectInfo = (
                ProjectPath: "",
                ProjectAssemblyName: "",
                ProjectDotNetVersion: "",
                ProjectTotalDocuments: 0,
                ProjectTotalPureDotNetDocuments: 0,
                ProjectTotalPureDotNetDocumentsLinesOfCode: 0,
                ProjectTotalScriptDocuments: 0,
                ProjectTotalClasses: 0,
                ProjectTotalDataClasses: 0,
                ProjectTotalMethods: 0
                );

            projectInfo.ProjectPath = _workspace.GetResourceName(p.FilePath);
            projectInfo.ProjectAssemblyName = p.AssemblyName;
            projectInfo.ProjectDotNetVersion = _projectAnalyzer.ParseDotNetVersion(p.FilePath);
            projectInfo.ProjectTotalDocuments = p.Documents.Count();
            projectInfo.ProjectTotalPureDotNetDocuments = p.Documents.Count(d => d.SourceCodeKind == SourceCodeKind.Regular);
            projectInfo.ProjectTotalScriptDocuments = p.Documents.Count(d => d.SourceCodeKind == SourceCodeKind.Script);

            var docSummary = _projectAnalyzer.GetDocumentSummary(p.Documents);

            projectInfo.ProjectTotalClasses = docSummary.NameTypeCount;
            projectInfo.ProjectTotalMethods = docSummary.MethodCount;
            projectInfo.ProjectTotalDataClasses = docSummary.DataOnlyNameTypeCount;
            projectInfo.ProjectTotalPureDotNetDocumentsLinesOfCode = docSummary.LinesOfCode;

            //We are going to summarize the references used in the project
            var referenceUsage = new Dictionary<string, (int ClassCount, int MethodCount, int LinesOfCode)>();

            docSummary.NameTypeInfo.Values.ToList().ForEach(c =>
            {
                foreach (var reference in c.References)
                {
                    if (!referenceUsage.ContainsKey(reference))
                    {
                        referenceUsage.Add(reference, (ClassCount: 0, MethodCount: 0, LinesOfCode: 0));
                    }
                    var currentReference = referenceUsage[reference];
                    currentReference.ClassCount++;
                    currentReference.LinesOfCode += c.LinesOfCode;
                    currentReference.MethodCount += c.MethodCount;
                    referenceUsage[reference] = currentReference;
                }
            });



            DataTable expandedReferenceUsage = new DataTable();
            expandedReferenceUsage.Columns.Add("ProjectPath", typeof(string));
            expandedReferenceUsage.Columns.Add("ProjectAssemblyName", typeof(string));
            expandedReferenceUsage.Columns.Add("ProjectDotNetVersion", typeof(string));
            expandedReferenceUsage.Columns.Add("ProjectTotalDocuments", typeof(int));
            expandedReferenceUsage.Columns.Add("ProjectTotalPureDotNetDocuments", typeof(int));
            expandedReferenceUsage.Columns.Add("ProjectTotalPureDotNetDocumentsLinesOfCode", typeof(int));
            expandedReferenceUsage.Columns.Add("ProjectTotalClasses", typeof(int));
            expandedReferenceUsage.Columns.Add("ProjectTotalMethods", typeof(int));
            expandedReferenceUsage.Columns.Add("ReferenceName", typeof(string));
            expandedReferenceUsage.Columns.Add("ApproximateNumberOfClassReferingTheReference", typeof(int));
            expandedReferenceUsage.Columns.Add("ApproximateNumberOfMethodsReferingTheReference", typeof(int));
            expandedReferenceUsage.Columns.Add("ApproximateNumberOfLinesOfCodeReferingTheReference", typeof(int));

            foreach (var reference in referenceUsage)
            {
                expandedReferenceUsage.Rows.Add(
                    projectInfo.ProjectPath,
                    projectInfo.ProjectAssemblyName,
                    projectInfo.ProjectDotNetVersion,
                    projectInfo.ProjectTotalDocuments,
                    projectInfo.ProjectTotalPureDotNetDocuments,
                    projectInfo.ProjectTotalPureDotNetDocumentsLinesOfCode,
                    projectInfo.ProjectTotalClasses,
                    projectInfo.ProjectTotalMethods,
                    reference.Key, //ReferenceName
                    reference.Value.ClassCount, //ApproximateNumberOfClassReferingTheReference
                    reference.Value.MethodCount, //ApproximateNumberOfMethodsReferingTheReference
                    reference.Value.LinesOfCode //ApproximateNumberOfLinesOfCodeReferingTheReference
                );
            }

            string repoName = _workspace.GetRepositoryName(p.FilePath);
            string segmentIdentifier = $"/{repoName}/{p.AssemblyName}";
            string resourceName = _workspace.GetResourceName(p.FilePath);

            IResourceMetricValue<DataTable> metric1 = new ResourceMetricValue<DataTable>
            (
                name: MetricName.DotNet.Project.PROJECT_LIBRARY_IMPACT_APPROXIMATION_EXPANDED,
                segment: segmentIdentifier,
                resource: resourceName,
                provider: FullQualifiedName,
                value: expandedReferenceUsage
            );
            result.Add(metric1);


            DataTable minimizedReferenceUsage = new DataTable();
            minimizedReferenceUsage.Columns.Add("ProjectPath", typeof(string));
            minimizedReferenceUsage.Columns.Add("ProjectAssemblyName", typeof(string));
            minimizedReferenceUsage.Columns.Add("ProjectDotNetVersion", typeof(string));
            minimizedReferenceUsage.Columns.Add("ProjectTotalDocuments", typeof(int));
            minimizedReferenceUsage.Columns.Add("ProjectTotalPureDotNetDocuments", typeof(int));
            minimizedReferenceUsage.Columns.Add("ProjectTotalPureDotNetDocumentsLinesOfCode", typeof(int));
            minimizedReferenceUsage.Columns.Add("ProjectTotalClasses", typeof(int));
            minimizedReferenceUsage.Columns.Add("ProjectTotalMethods", typeof(int));
            minimizedReferenceUsage.Columns.Add("ReferenceNames", typeof(string));


            string compactReferenceNames = string.Join("#", referenceUsage.Keys);


            minimizedReferenceUsage.Rows.Add(
                    projectInfo.ProjectPath,
                    projectInfo.ProjectAssemblyName,
                    projectInfo.ProjectDotNetVersion,
                    projectInfo.ProjectTotalDocuments,
                    projectInfo.ProjectTotalPureDotNetDocuments,
                    projectInfo.ProjectTotalPureDotNetDocumentsLinesOfCode,
                    projectInfo.ProjectTotalClasses,
                    projectInfo.ProjectTotalMethods,
                    compactReferenceNames);

            IResourceMetricValue<DataTable> metric2 = new ResourceMetricValue<DataTable>
           (
               name: MetricName.DotNet.Project.PROJECT_LIBRARY_IMPACT_APPROXIMATION,
               segment: segmentIdentifier,
               resource: resourceName,
               provider: FullQualifiedName,
               value: minimizedReferenceUsage
           );
            result.Add(metric2);

            return result;
        }



        private HashSet<string> GetDBRelatedNamespaces()
        {
            HashSet<string> dbRelatedNamespaces = new HashSet<string>();
            if (!string.IsNullOrEmpty(_config.NamespaceListFileToIdentifyDBRelatedClasses))
            {
                try
                {
                    if (!File.Exists(_config.NamespaceListFileToIdentifyDBRelatedClasses))
                    {
                        _logger.LogError("Namespace list file to identify DB related classes does not exist: {FilePath}", _config.NamespaceListFileToIdentifyDBRelatedClasses);
                        return dbRelatedNamespaces;
                    }
                    var lines = File.ReadAllLines(_config.NamespaceListFileToIdentifyDBRelatedClasses);
                    foreach (var line in lines)
                    {
                        var entry = line.Trim();

                        if (!string.IsNullOrWhiteSpace(entry) && !entry.StartsWith("#"))
                        {
                            dbRelatedNamespaces.Add(line.Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error reading DB related namespaces from file: {FilePath}", _config.NamespaceListFileToIdentifyDBRelatedClasses, ex);
                }
            }
            return dbRelatedNamespaces;
        }

        private static IEnumerable<string> GetAllNamespaces(INamespaceSymbol namespaceSymbol)
        {
            if (!string.IsNullOrEmpty(namespaceSymbol.Name))
            {
                yield return namespaceSymbol.ToDisplayString();
            }

            foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                foreach (var childNamespaceName in GetAllNamespaces(childNamespace))
                {
                    yield return childNamespaceName;
                }
            }
        }


    }
}
