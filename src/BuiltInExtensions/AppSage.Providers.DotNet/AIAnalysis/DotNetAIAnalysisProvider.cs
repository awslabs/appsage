using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Resource;
using AppSage.Infrastructure.AI;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Data;
using AppSage.Core.Configuration;
using System.Text.RegularExpressions;
using AppSage.Core.Workspace;

namespace AppSage.Providers.DotNet.AIAnalysis
{
    public class DotNetAIAnalysisProvider: IMetricProvider
    {
        private IAppSageLogger _logger;
        private IAppSageWorkspace _workspace;
        private IServiceProvider _serviceProvider;
        private IAppSageConfiguration _configuration;
        private IAIQuery _aiQuery;
        public string FullQualifiedName => GetType().FullName;

        public string Description => "Provide advaned .NET analysis with AI";

        public DotNetAIAnalysisProvider(IAppSageLogger logger, IServiceProvider serviceProvider, IAppSageWorkspace workspace, IAppSageConfiguration configuration)
        {

            _logger = logger;
            _workspace = workspace;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _aiQuery = _serviceProvider.GetServices<IAIQuery>().Where(q => q.GetType() == typeof(BedrockService)).FirstOrDefault();

        }

        public void Run(IMetricCollector metrics)
        {
            try
            {
                ConcurrentBag<ProjectSummary> projectSummaryList = new ConcurrentBag<ProjectSummary>();


                IResourceProvider soutionFileProvider = new SolutionFileProvider(_logger, _workspace);
                var slnFileList = soutionFileProvider.GetResources().ToList();

                ConcurrentDictionary<string, HashSet<string>> projectSolutionMapping = new ConcurrentDictionary<string, HashSet<string>>();

                if (!MSBuildLocator.IsRegistered)
                {
                    var msBuildPath = _configuration.Get<string>("AppSage.Providers.DotNet.SHARED:MSBuildPath");
                    MSBuildLocator.RegisterMSBuildPath(msBuildPath);
                }

                List<Task> jobs = new List<Task>();


                foreach (string slnFile in slnFileList.Select(r => r.Path))
                {
                    string solutionRelativePath = _workspace.GetResourceName(slnFile);
                    if (!projectSolutionMapping.ContainsKey(slnFile))
                    {
                        projectSolutionMapping[solutionRelativePath] = new HashSet<string>();
                    }
                    using (var workspace = MSBuildWorkspace.Create())
                    {
                        // Load the solution
                        Task task = workspace.OpenSolutionAsync(slnFile).ContinueWith(task =>
                        {
                            if (task.IsCompletedSuccessfully)
                            {
                                _logger.LogInformation("Processing solution: {SlnFile}", slnFile);


                                var solution = task.Result;
                                _logger.LogInformation("Opened solution: {SlnFile} with {ProjectCount} projects.", slnFile, solution.Projects.Count());

                                int projectMaxParallelism = _configuration.Get<int>("AppSage.Providers.DotNet.AIAnalysis.DotNetAIAnalysisProvider:ProjectMaxParallelism");

                                solution.Projects.AsParallel().WithDegreeOfParallelism(projectMaxParallelism).ForAll(project =>
                                {
                                    _logger.LogInformation("Analyzing project : {ProjectName}", project.Name);
                                    projectSolutionMapping[solutionRelativePath].Add(_workspace.GetResourceName(project.FilePath));
                                    var result = SummarizeProject(project);
                                    projectSummaryList.Add(result);
                                });
                            }
                            else
                            {
                                _logger.LogError("Failed to open solution {SlnFile}: {ErrorMessage}", slnFile, task.Exception?.Message);
                            }
                            _logger.LogInformation("Processed solution: {SlnFile}", slnFile);
                        });
                        //Let's now overcomplicate things and analyze one solution at a time. 
                        task.Wait();
                    }
                }
                _logger.LogInformation("{FullQualifiedName}:Solution Analysis:[Completed]", FullQualifiedName);

                jobs.Clear();


                //we will now process those projects that are not part of any solution
                HashSet<string> projectsWithSoutions = new HashSet<string>();
                projectsWithSoutions.UnionWith(projectSolutionMapping.Values.SelectMany(kvp => kvp));
                IResourceProvider projectFileProvider = new ProjectFileProvider(_logger, _workspace);
                var projectFileList = projectFileProvider.GetResources().ToList();

                projectSolutionMapping[ConstString.UNDEFINED] = new HashSet<string>();

                foreach (var projectFile in projectFileList)
                {
                    _logger.LogInformation("Processing project file: {ProjectFileName}", projectFile.Name);
                    if (!projectsWithSoutions.Contains(projectFile.Name))
                    {
                        projectSolutionMapping[ConstString.UNDEFINED].Add(projectFile.Name);

                        using (var workspace = MSBuildWorkspace.Create())
                        {
                            var task = workspace.OpenProjectAsync(projectFile.Path).ContinueWith(task =>
                            {
                                if (task.IsCompletedSuccessfully)
                                {
                                    var project = task.Result;
                                    var result = SummarizeProject(project);
                                    projectSummaryList.Add(result);
                                }
                                else
                                {
                                    _logger.LogError("Failed to open project {ProjectFileName}: {ErrorMessage}", projectFile.Name, task.Exception?.Message);
                                }
                            });
                            jobs.Add(task);
                        }
                    }
                    _logger.LogInformation("Processed project file: {ProjectFileName}", projectFile.Name);
                }

                jobs.ForEach(t => t.Wait());
                _logger.LogInformation("{FullQualifiedName}:Project Analysis:[Completed]", FullQualifiedName);

                GetMetrics(projectSummaryList).ForEach(m => metrics.Add(m));
            }
            finally { 
                metrics.CompleteAdding();
            }
        }



        private List<IMetric> GetMetrics(ConcurrentBag<ProjectSummary> projectList)
        {
            List<IMetric> metrics = new List<IMetric>();

            foreach (var project in projectList)
            {
                // Create project-level AI summary metrics
                CreateProjectSummaryMetrics(project, metrics);

                // Create document-level AI summary metrics for important documents
                foreach (var document in project.Documents)
                {
                    bool isWorthCollectingAsMetric = DetermineDocumentImportance(document);

                    if (isWorthCollectingAsMetric)
                    {
                        metrics.Add(new MetricValue<string>
                        (
                            name: MetricName.DotNet.Project.Document.AISummary,
                            segment: project.SegmentIdentifier,
                            resource: document.FilePath,
                            provider: FullQualifiedName,
                            value: document.Summary
                        ));
                    }
                }
            }

            // Create overarching summary of all projects
            string overarchingSummary = CreateOverarchingSummary(projectList.ToList());

            metrics.Add(new MetricValue<string>
            (
                name: MetricName.DotNet.AISummary,
                segment: ConstString.UNDEFINED,
                resource: ConstString.UNDEFINED,
                provider: FullQualifiedName,
                value: overarchingSummary
            ));

            return metrics;
        }

        private void CreateProjectSummaryMetrics(ProjectSummary project, List<IMetric> metrics)
        {
            // Create a comprehensive project summary from all the segmented summaries
            string consolidatedProjectSummary = CreateConsolidatedProjectSummary(project);

            metrics.Add(new MetricValue<string>
            (
                name: MetricName.DotNet.Project.AISummary,
                segment: project.SegmentIdentifier,
                resource: project.ResourceName,
                provider: FullQualifiedName,
                value: consolidatedProjectSummary
            ));

            // Add individual summary aspects as separate metrics if they have content
            if (!string.IsNullOrEmpty(project.Summary.OverallSummary))
            {
                metrics.Add(new MetricValue<string>
                (
                    name: $"{MetricName.DotNet.Project.AISummary}.Overall",
                    segment: project.SegmentIdentifier,
                    resource: project.ResourceName,
                    provider: FullQualifiedName,
                    value: project.Summary.OverallSummary
                ));
            }

            if (!string.IsNullOrEmpty(project.Summary.ArchitectureSummary))
            {
                metrics.Add(new MetricValue<string>
                (
                    name: $"{MetricName.DotNet.Project.AISummary}.Architecture",
                    segment: project.SegmentIdentifier,
                    resource: project.ResourceName,
                    provider: FullQualifiedName,
                    value: project.Summary.ArchitectureSummary
                ));
            }

            if (!string.IsNullOrEmpty(project.Summary.ChallengesSummary))
            {
                metrics.Add(new MetricValue<string>
                (
                    name: $"{MetricName.DotNet.Project.AISummary}.ModernizationRisks",
                    segment: project.SegmentIdentifier,
                    resource: project.ResourceName,
                    provider: FullQualifiedName,
                    value: project.Summary.ChallengesSummary
                ));
            }
        }

        private string CreateConsolidatedProjectSummary(ProjectSummary project)
        {
            var summaryParts = new List<string>();

            if (!string.IsNullOrEmpty(project.Summary.OverallSummary))
                summaryParts.Add($"Purpose: {project.Summary.OverallSummary}");

            if (!string.IsNullOrEmpty(project.Summary.ArchitectureSummary))
                summaryParts.Add($"Architecture: {project.Summary.ArchitectureSummary}");

            if (!string.IsNullOrEmpty(project.Summary.TechnologySummary))
                summaryParts.Add($"Technologies: {project.Summary.TechnologySummary}");

            if (!string.IsNullOrEmpty(project.Summary.DesignPatternsSummary))
                summaryParts.Add($"Design Patterns: {project.Summary.DesignPatternsSummary}");

            if (!string.IsNullOrEmpty(project.Summary.CodeQualitySummary))
                summaryParts.Add($"Code Quality: {project.Summary.CodeQualitySummary}");

            if (!string.IsNullOrEmpty(project.Summary.ChallengesSummary))
                summaryParts.Add($"Modernization Risks: {project.Summary.ChallengesSummary}");

            if (!string.IsNullOrEmpty(project.Summary.FutureWorkSummary))
                summaryParts.Add($"Business Value: {project.Summary.FutureWorkSummary}");

            return string.Join("\n\n", summaryParts);
        }

        private bool DetermineDocumentImportance(DocumentSummary document)
        {
            var fileName = Path.GetFileName(document.FilePath).ToLowerInvariant();

            // Check if this is a core architectural file
            bool isArchitecturallyImportant = fileName.Contains("controller") ||
                                            fileName.Contains("service") ||
                                            fileName.Contains("repository") ||
                                            fileName.Contains("manager") ||
                                            fileName.Contains("handler") ||
                                            fileName.Contains("startup") ||
                                            fileName.Contains("program") ||
                                            fileName.Contains("config");

            // Check if the summary indicates important patterns or issues
            bool hasImportantContent = !string.IsNullOrEmpty(document.Summary) &&
                                     (document.Summary.ToLower().Contains("pattern") ||
                                      document.Summary.ToLower().Contains("architecture") ||
                                      document.Summary.ToLower().Contains("risk") ||
                                      document.Summary.ToLower().Contains("legacy") ||
                                      document.Summary.ToLower().Contains("dependency"));

            return isArchitecturallyImportant || hasImportantContent;
        }

        private string CreateOverarchingSummary(List<ProjectSummary> projectList)
        {
            try
            {
                if (!projectList.Any())
                    return "No projects analyzed.";

                // Collect high-level insights from all projects
                var projectInsights = projectList
                    .Where(p => !string.IsNullOrEmpty(p.Summary.OverallSummary))
                    .Select(p => $"Project: {p.AssemblyName}\n{p.Summary.OverallSummary}")
                    .Take(10); // Limit to avoid token limits

                var combinedInsights = string.Join("\n\n---\n\n", projectInsights);

                var prompt = $@"
Based on analysis of multiple .NET projects in this codebase, provide a comprehensive architectural assessment:

Project Summaries:
{combinedInsights}

Provide a strategic analysis covering:

1. SOLUTION_PURPOSE: What can users accomplish with this application suite?
2. BUSINESS_FUNCTIONS: What business capabilities does this system provide?
3. USE_CASES: What are the primary use cases and user workflows?
4. OVERALL_ARCHITECTURE: How are the projects structured and interconnected?
5. TECHNOLOGY_STACK: What technologies are used? Identify legacy vs modern components.
6. DESIGN_ISSUES: What bad design patterns or anti-patterns are observed?
7. MODERNIZATION_RISKS: What are the key risks and challenges in modernizing this codebase?
8. DEPENDENCIES: How are projects interconnected and what are the dependency concerns?
9. RECOMMENDATIONS: What architectural improvements should be prioritized?

Focus on insights that would help a software architect plan modernization and refactoring efforts.
Keep each section concise but actionable.";

                return _aiQuery.Invoke(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating overarching summary: {ErrorMessage}", ex.Message);
                return $"Error generating overarching summary: {ex.Message}";
            }
        }

        private ProjectSummary SummarizeProject(Project p)
        {
            string repoName = _workspace.GetRepositoryName(p.FilePath ?? "");
            string segmentIdentifier = $"/{repoName}/{p.AssemblyName}";
            string resourceName = _workspace.GetResourceName(p.FilePath ?? "");

            ProjectSummary projectSummary = new ProjectSummary(resourceName, segmentIdentifier, p.AssemblyName, p.FilePath ?? "");

            try
            {
                // Step 1: Analyze project metadata and structure
                projectSummary.Summary.TechnologySummary = AnalyzeProjectMetadata(p);

                // Step 2: Filter and categorize documents
                var importantDocuments = FilterImportantDocuments(p.Documents);

                int maxConcurrency = _configuration.Get<int>("AppSage.Providers.DotNet.AIAnalysis.DotNetAIAnalysisProvider:AnalyzeDocumentWithAIMaxParallelism");

                importantDocuments.AsParallel().WithDegreeOfParallelism(maxConcurrency).ForAll(doc =>
                {
                    try
                    {
                        var docSummary = AnalyzeDocumentWithAI(doc);
                        if (docSummary != null)
                        {
                            lock (projectSummary.Documents)
                            {
                                projectSummary.Documents.Add(docSummary);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error analyzing document {DocFilePath}: {ErrorMessage}", doc.FilePath, ex.Message);
                    }
                });


                // Step 4: Synthesize project-level summary
                SynthesizeProjectSummary(projectSummary);

                _logger.LogInformation("Completed analysis for project: {ProjectName} with {DocumentCount} important documents", p.Name, projectSummary.Documents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error analyzing project {ProjectName}: {ErrorMessage}", p.Name, ex.Message);
            }

            return projectSummary;
        }

        private string AnalyzeProjectMetadata(Project project)
        {
            try
            {
                // Collect project metadata
                var metadata = new
                {
                    ProjectName = project.Name,
                    project.AssemblyName,
                    OutputType = project.CompilationOptions?.OutputKind.ToString() ?? "Unknown",
                    TargetFramework = project.ParseOptions?.DocumentationMode.ToString(),
                    HasReferences = project.ProjectReferences.Any(),
                    DocumentCount = project.Documents.Count(),
                    Languages = project.Documents.Select(d => d.SourceCodeKind.ToString()).Distinct().ToList(),
                    FileExtensions = project.Documents.Select(d => Path.GetExtension(d.FilePath)).Distinct().ToList()
                };

                // Get project file content for dependencies analysis
                string projectFileContent = "";
                if (File.Exists(project.FilePath))
                {
                    projectFileContent = File.ReadAllText(project.FilePath);
                }
                var sanitizedContent = CleanXmlWhitespace(projectFileContent);

                var prompt = $@"
Analyze this .NET project metadata and provide initial insights:

Project Metadata:
- ReferenceName: {metadata.ProjectName}
- Assembly: {metadata.AssemblyName}  
- Output Type: {metadata.OutputType}
- Document Count: {metadata.DocumentCount}
- File Extensions: {string.Join(", ", metadata.FileExtensions)}

Project File Content:```{sanitizedContent}```

Based on this information, provide a very brief analysis covering:
1. Project type and purpose (web app, library, console, etc.)
2. Technologies and frameworks used
3. Architecture hints from dependencies

Keep response concise and focused.";

                string response = _aiQuery.Invoke(prompt);
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError("Error analyzing project metadata for {ProjectName}: {ErrorMessage}", project.Name, ex.Message);
                return "";
            }
        }

        public static string CleanXmlWhitespace(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Pattern explanation:
            // (?<=>)     - Positive lookbehind for closing >
            // \s+        - One or more whitespace characters (spaces, tabs, newlines, carriage returns)
            // (?=<)      - Positive lookahead for opening <
            // This matches whitespace that appears between XML tags: >...whitespace...<

            string pattern = @"(?<=>)\s+(?=<)";

            // Replace all whitespace between tags with empty string
            string result = Regex.Replace(input, pattern, string.Empty, RegexOptions.Multiline);

            // Optional: Clean up excessive whitespace within tag content while preserving attribute spacing
            // This removes leading/trailing whitespace from text content between tags
            string contentPattern = @"(?<=>)\s*([^<]+?)\s*(?=<)";
            result = Regex.Replace(result, contentPattern, match =>
            {
                string content = match.Groups[1].Value;
                // Only trim if it's not just whitespace (preserve intentional spacing)
                return string.IsNullOrWhiteSpace(content) ? string.Empty : content.Trim();
            }, RegexOptions.Multiline);

            return result;
        }

        private IEnumerable<Document> FilterImportantDocuments(IEnumerable<Document> documents)
        {
            var filteredDocs = new List<Document>();

            int mdp = _configuration.Get<int>("AppSage.Providers.DotNet.AIAnalysis.DotNetAIAnalysisProvider:FilterImportantDocumentsMaxParallelism");

            documents.AsParallel().WithDegreeOfParallelism(mdp).ForAll(doc =>
            {
                if (IsDocumentWorthAnalyzing(doc))
                {
                    lock (filteredDocs)
                    {
                        filteredDocs.Add(doc);
                    }
                }
            });


            // Prioritize by importance - controllers, services, models first
            return filteredDocs;
        }

        /// <summary>
        /// Checks if a document is worth analyzing based on its file path, name, and content.
        /// This is a simple text-based heuristic to filter out common library files, auto-generated files, and non-code files.
        /// </summary>
        /// <param name="document">Document to analyze</param>
        /// <returns>True if it's worth analyzing false otherwise</returns>
        private bool IsDocumentWorthAnalyzing(Document document)
        {
            if (string.IsNullOrEmpty(document.FilePath))
                return false;

            var fileName = Path.GetFileName(document.FilePath).ToLowerInvariant();
            var extension = Path.GetExtension(document.FilePath).ToLowerInvariant();
            var directory = Path.GetDirectoryName(document.FilePath)?.ToLowerInvariant() ?? "";

            // Skip common library files
            var skipPatterns = new[]
            {
                "jquery", "bootstrap", "angular", "react", "vue", "lodash", "underscore",
                "moment", "chart", "d3", "three", "babylonjs", "pixi",
                ".min.", "vendor", "lib", "third-party", "thirdparty", "external",
                "packages", "node_modules", "bower_components",
                "assemblyinfo", "globalassemblyinfo", "assemblyattributes"
            };

            if (skipPatterns.Any(pattern => fileName.Contains(pattern) || directory.Contains(pattern)))
                return false;

            // Skip auto-generated files
            var autoGenPatterns = new[]
            {
                ".designer.", ".generated.", ".g.", ".i.",
                "temporarygenerated", "temp_", "__"
            };

            if (autoGenPatterns.Any(pattern => fileName.Contains(pattern)))
                return false;

            // Only analyze code files
            var codeExtensions = new[]
            {
                ".cs", ".vb", ".fs", ".cpp", ".c", ".h",           // Core code files
                ".aspx", ".ascx", ".master",                        // Web Forms
                ".cshtml", ".vbhtml", ".razor",                     // Razor pages/views
                ".xaml",                                            // WPF/UWP/MAUI
                ".config", ".json", ".xml",                         // Configuration files
                ".js", ".ts",                                       // JavaScript/TypeScript
                ".sql",                                             // SQL scripts
            };
            return codeExtensions.Contains(extension);
        }

        private DocumentSummary? AnalyzeDocumentWithAI(Document document)
        {
            try
            {
                var sourceText = document.GetTextAsync().Result;
                if (sourceText == null || sourceText.Length > 50000) // Skip very large files
                    return null;

                var content = sourceText.ToString();
                var fileName = Path.GetFileName(document.FilePath);

                var prompt = $@"
Analyze this .NET code file and provide architectural insights:

File: {fileName}
Code:
{content}

Provide analysis in this exact format:
SUMMARY: [Brief description of what this file does]
PATTERNS: [Design patterns used, if any]
DEPENDENCIES: [Key dependencies or coupling concerns]
QUALITY: [Code quality observations]
RISKS: [Modernization risks or technical debt]

Keep each section to 1-2 sentences max.";

                string response = _aiQuery.Invoke(prompt);

                return new DocumentSummary
                {
                    FilePath = document.FilePath ?? "",
                    Summary = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error analyzing document {DocumentFilePath}: {ErrorMessage}", document.FilePath, ex.Message);
                return null;
            }
        }

        private void SynthesizeProjectSummary(ProjectSummary projectSummary)
        {
            try
            {
                if (!projectSummary.Documents.Any())
                    return;

                var documentSummaries = projectSummary.Documents
                    .Select(d => $"File: {Path.GetFileName(d.FilePath)}\n{d.Summary}")
                    .Take(20); // Limit context

                var combinedSummaries = string.Join("\n\n---\n\n", documentSummaries);

                var prompt = $@"
Based on these individual file analyses from project '{projectSummary.AssemblyName}', provide a comprehensive project summary:

{combinedSummaries}

Provide analysis in this structured format:

OVERALL_PURPOSE: [What this project does and its main purpose]
ARCHITECTURE: [Architectural patterns and structure observed]  
TECHNOLOGIES: [Technologies used, identify legacy vs modern]
DESIGN_PATTERNS: [Design patterns observed, good and bad]
DEPENDENCIES: [Dependency structure and coupling]
CODE_QUALITY: [Overall code quality assessment]
MODERNIZATION_RISKS: [Risks in modernizing this codebase]
BUSINESS_VALUE: [Business functionality this project provides]

Keep each section concise but informative.";

                string response = _aiQuery.Invoke(prompt);

                // Parse the structured response
                ParseStructuredResponse(response, projectSummary.Summary);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error synthesizing project summary for {AssemblyName}: {ErrorMessage}", projectSummary.AssemblyName, ex.Message);
            }
        }

        private void ParseStructuredResponse(string response, ProjectSummary.SegmentSummary summary)
        {
            try
            {
                var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (line.StartsWith("OVERALL_PURPOSE:"))
                        summary.OverallSummary = line.Substring("OVERALL_PURPOSE:".Length).Trim();
                    else if (line.StartsWith("ARCHITECTURE:"))
                        summary.ArchitectureSummary = line.Substring("ARCHITECTURE:".Length).Trim();
                    else if (line.StartsWith("TECHNOLOGIES:"))
                        summary.TechnologySummary = line.Substring("TECHNOLOGIES:".Length).Trim();
                    else if (line.StartsWith("DESIGN_PATTERNS:"))
                        summary.DesignPatternsSummary = line.Substring("DESIGN_PATTERNS:".Length).Trim();
                    else if (line.StartsWith("DEPENDENCIES:"))
                        summary.DependencySummary = line.Substring("DEPENDENCIES:".Length).Trim();
                    else if (line.StartsWith("CODE_QUALITY:"))
                        summary.CodeQualitySummary = line.Substring("CODE_QUALITY:".Length).Trim();
                    else if (line.StartsWith("MODERNIZATION_RISKS:"))
                        summary.ChallengesSummary = line.Substring("MODERNIZATION_RISKS:".Length).Trim();
                    else if (line.StartsWith("BUSINESS_VALUE:"))
                        summary.FutureWorkSummary = line.Substring("BUSINESS_VALUE:".Length).Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error parsing structured response: {ErrorMessage}", ex.Message);
            }
        }

        public class ProjectSummary
        {
            public string ResourceName { get; set; }
            public string SegmentIdentifier { get; set; }
            public string AssemblyName { get; set; }
            public string FilePath { get; set; }
            public SegmentSummary Summary { get; set; } = new SegmentSummary();

            public ProjectSummary(string resourceName, string segmentIdentifier, string assemblyName, string filePath)
            {
                ResourceName = resourceName;
                SegmentIdentifier = segmentIdentifier;
                AssemblyName = assemblyName;
                FilePath = filePath;
            }
            public List<DocumentSummary> Documents { get; set; } = new List<DocumentSummary>();

            public class SegmentSummary
            {
                public string? OverallSummary { get; set; }
                public string? ArchitectureSummary { get; set; }
                public string? DesignPatternsSummary { get; set; }
                public string? TechnologySummary { get; set; }
                public string? DependencySummary { get; set; }
                public string? ChallengesSummary { get; set; }
                public string? FutureWorkSummary { get; set; }
                public string? CodeQualitySummary { get; set; }
                public string? PerformanceSummary { get; set; }
                public string? SecuritySummary { get; set; }
                public string? DocumentationSummary { get; set; }
                public string? CodeStructureSummary { get; set; }
                public string? CodeStyleSummary { get; set; }
                public string? TestingSummary { get; set; }
            }
        }

        public class DocumentSummary
        {
            public string FilePath { get; set; } = "";
            public string Summary { get; set; } = "";
        }
    }


}
