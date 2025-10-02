using AppSage.Core.ComplexType.Graph;
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
using Newtonsoft.Json;
using System.Data;
using System.Reflection;
namespace AppSage.Providers.DotNet.DependencyAnalysis
{
    public class DotNetDependencyAnalysisProvider : IMetricProvider
    {
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        Utility _utility;
        ProjectAnalyzer _projectAnalyzer;
        AssemblyAnalyzer _assemblyAnalyzer;


        private (
            int SolutionMaxParallelism,
            int ProjectMaxParallelism,
            int DocumentMaxParallelism,
            string MSBuildPath,
            int LargeMetricDependencyGraphNodeThreshold,
            int LargeMetricDependencyGraphEdgeThreshold,
            string[] NamespacePrefixToInclude,
            string[] NamespacePrefixToExclude
            ) _config;
        public DotNetDependencyAnalysisProvider(IAppSageLogger logger, IAppSageWorkspace workspace, IAppSageConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            _utility = new Utility(_logger, _workspace);
            _projectAnalyzer = new ProjectAnalyzer(_logger, configuration);
            _assemblyAnalyzer = new AssemblyAnalyzer(_logger, cacheResult: true);
            _config.MSBuildPath = configuration.Get<string>("AppSage.Providers.DotNet.SHARED:MSBuildPath");
            _config.ProjectMaxParallelism = configuration.Get<int>("AppSage.Providers.DotNet.DependenyAnalysis.DotNetDependencyAnalysisProvider:ProjectMaxParallelism");
            _config.LargeMetricDependencyGraphNodeThreshold = configuration.Get<int>("AppSage.Providers.DotNet.DependenyAnalysis.DotNetDependencyAnalysisProvider:LargeMetricDependencyGraphNodeThreshold");
            _config.LargeMetricDependencyGraphEdgeThreshold = configuration.Get<int>("AppSage.Providers.DotNet.DependenyAnalysis.DotNetDependencyAnalysisProvider:LargeMetricDependencyGraphEdgeThreshold");
            _config.SolutionMaxParallelism = configuration.Get<int>("AppSage.Providers.DotNet.DependenyAnalysis.DotNetDependencyAnalysisProvider:SolutionMaxParallelism");
            _config.NamespacePrefixToInclude = configuration.Get<string[]>("AppSage.Providers.DotNet.DependenyAnalysis.DotNetDependencyAnalysisProvider:NamespacePrefixToInclude");
            _config.NamespacePrefixToExclude = configuration.Get<string[]>("AppSage.Providers.DotNet.DependenyAnalysis.DotNetDependencyAnalysisProvider:NamespacePrefixToExclude");
        }

        public string FullQualifiedName => GetType().FullName;

        public string Description => "Provides advanced .NET code dependency analysis";

        public void Run(IMetricCollector collector)
        {
            try
            {
                if (!MSBuildLocator.IsRegistered)
                {
                    var msBuildPath = _config.MSBuildPath;
                    MSBuildLocator.RegisterMSBuildPath(msBuildPath);
                }
                System.Data.DataTable dt = new System.Data.DataTable();
                dt.Columns.Add("Test", typeof(string));
           
                dt.AcceptChanges();

                var projectSolutionMapping = GetProjectSolutionMapping();
                var projectSolutionMappingTable = GetProjectSolutionMappingTable(projectSolutionMapping);

                IMetricValue<DataTable> solutionProjectMappingMetric =
                    new MetricValue<DataTable>
                    (
                        name: MetricName.DotNet.SOLUTION_PROJECT_MAPPING,
                        segment: ConstString.AGGREGATE,
                        provider: FullQualifiedName,
                        value: projectSolutionMappingTable
                    );
                collector.Add(solutionProjectMappingMetric);

                var projectSolutionMappingGraph = GetProjectSolutionMappingGraph(projectSolutionMapping);


                IResourceProvider projectFileProvider = new ProjectFileProvider(_logger, _workspace);
                var projectFileList = projectFileProvider.GetResources().ToList();

                projectFileList.AsParallel().WithDegreeOfParallelism(_config.ProjectMaxParallelism).ForAll(projectFile =>
                {
                    _logger.LogInformation("Processing project file: {ProjectFileName}", projectFile.Name);
                    try
                    {
                        using (var workspace = MSBuildWorkspace.Create())
                        {

                            var openJob = workspace.OpenProjectAsync(projectFile.Path).ContinueWith(task =>
                            {
                                if (task.IsCompletedSuccessfully)
                                {
                                    var project = task.Result;
                                    var result = GetProjectDependencies(project);

                                    string currentProjectNodeId = _utility.GetNodeIdProject(project);
                                    INode currentProjectNode = result.GetNode(currentProjectNodeId);
                                    IEnumerable<INode> solutionsReferingTheCurrentProject = projectSolutionMappingGraph.GetPredecessors(currentProjectNode).Where(n => n.Type == ConstString.Dependency.NodeType.SOLUTION);
                                    foreach (var sn in solutionsReferingTheCurrentProject)
                                    {
                                        //solution can have only one repository
                                        var solutionRepoNode = projectSolutionMappingGraph.GetAdjacentNodes(sn).Where(n => n.Type == ConstString.Dependency.NodeType.REPOSITORY).First();

                                        //we create a new node to prevent getting large result per project
                                        //The level of merge we do keeps are 1. solution repo node, 2. solution node and 3. all dependencies of current project. 
                                        INode solutionNode = result.AddOrUpdateNode(sn.Id, sn.Name, ConstString.Dependency.NodeType.SOLUTION, sn.Attributes);
                                        INode cloneSoutionRepoNode = result.AddOrUpdateNode(solutionRepoNode.Id, solutionRepoNode.Name, ConstString.Dependency.NodeType.REPOSITORY, solutionRepoNode.Attributes);
                                        result.AddOrUpdateEdge(cloneSoutionRepoNode, solutionNode, ConstString.Dependency.DependencyType.RESIDE);
                                        result.AddOrUpdateEdge(solutionNode, currentProjectNode, ConstString.Dependency.DependencyType.REFER);
                                    }

                                    //We create a new metric result per project to avoid large graphs in a single metric
                                    //This allows later filtering and aggregation easy
                                    string repoName = _workspace.GetRepositoryName(project.FilePath);
                                    string segmentIdentifier = $"/{repoName}/{project.AssemblyName}";
                                    string resourceName = _workspace.GetResourceName(project.FilePath);
                                    var metricValue = new ResourceMetricValue<DirectedGraph>(
                                        name: MetricName.DotNet.Project.CODE_DEPENDENCY_GRAPH,
                                           segment: segmentIdentifier,
                                           resource: resourceName,
                                           provider: FullQualifiedName,
                                           value: result
                                         );

                                    if (result.Nodes.Count > _config.LargeMetricDependencyGraphNodeThreshold || result.Edges.Count > _config.LargeMetricDependencyGraphEdgeThreshold)
                                    {
                                        metricValue.IsLargeMetric = true;
                                    }
                                    collector.Add(metricValue);
                                }
                                else
                                {
                                    _logger.LogError("Failed to open project {ProjectFileName}: {ErrorMessage}", projectFile.Name, task.Exception?.Message);
                                }

                            });
                            openJob.Wait();
                            _logger.LogInformation("Processed project file: {ProjectFileName}", projectFile.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error processing project file {ProjectFileName}: {ErrorMessage}", projectFile.Name, ex.Message);
                        return;
                    }
                });
            }
            finally { collector.CompleteAdding(); }
        }

        private IEnumerable<(string SolutionRepoName, string SolutionNodeId, HashSet<(string ProjectRepoName, string ProjectNodeId)> ProjectSet)> GetProjectSolutionMapping()
        {

            List<(string SolutionRepoName, string SolutionNodeId, HashSet<(string ProjectRepoName, string ProjectNodeId)> ProjectSet)> result = new();
            IResourceProvider soutionFileProvider = new SolutionFileProvider(_logger, _workspace);
            var slnFileList = soutionFileProvider.GetResources().ToList();




            slnFileList.AsParallel().WithDegreeOfParallelism(_config.SolutionMaxParallelism).ForAll(slnFile =>
            {
                _logger.LogInformation("Processing solution file: {SolutionFileName}", slnFile.Name);

                try
                {
                    using (var workspace = MSBuildWorkspace.Create())
                    {
                        // Load the solution
                        Task openJob = workspace.OpenSolutionAsync(slnFile.Path).ContinueWith(task =>
                        {
                            if (task.IsCompletedSuccessfully)
                            {
                                _logger.LogInformation("Processing solution: {SolutionFile}", slnFile);
                                Solution solution = task.Result;
                                string solutionNodeId = _utility.GetNodeIdSolution(solution);
                                string solutionRepoName = _workspace.GetRepositoryName(slnFile.Path);

                                HashSet<(string RepoName, string ProjectNodeId)> projectSet = new HashSet<(string RepoName, string ProjectNodeId)>();

                                _logger.LogInformation("Opened solution: {SolutionFile} with {ProjectCount} projects.", slnFile, solution.Projects.Count());
                                foreach (var project in solution.Projects)
                                {
                                    string projectNodeId = _utility.GetNodeIdProject(project);
                                    string projectRepoName = _workspace.GetResourceName(project.FilePath);
                                    _logger.LogInformation("Analyzing project : {ProjectName}", project.Name);
                                    projectSet.Add((projectRepoName, projectNodeId));
                                }
                                lock (result)
                                {
                                    result.Add((solutionRepoName, solutionNodeId, projectSet));
                                }
                            }
                            else
                            {
                                _logger.LogError("Failed to open solution {SolutionFile}", slnFile, task.Exception);
                            }
                            _logger.LogInformation("Processed the solution: {SolutionFile}", slnFile);
                        });
                        openJob.Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error processing solution file {SolutionFileName}", slnFile.Name, ex);
                    return;
                }
            });
            _logger.LogInformation("{FullQualifiedName}:Solution Analysis:[Completed]", FullQualifiedName);
            return result;
        }

        private DirectedGraph GetProjectSolutionMappingGraph(IEnumerable<(string SolutionRepoName, string SolutionNodeId, HashSet<(string ProjectRepoName, string ProjectNodeId)> ProjectSet)> mapping)
        {
            DirectedGraph result = new DirectedGraph();
            foreach (var item in mapping)
            {
                //get the last part of the solution path / as the node name
                string slnName = Path.GetFileNameWithoutExtension(item.SolutionNodeId);
                var solutionAttributes = new Dictionary<string, string>()
                {
                    [ConstString.Dependency.Attributes.RepositoryName] = item.SolutionRepoName
                };
                var solutionNode = result.AddOrUpdateNode(item.SolutionNodeId, slnName, ConstString.Dependency.NodeType.SOLUTION, solutionAttributes);
                var solutionRepoNode = result.AddOrUpdateNode(item.SolutionRepoName, item.SolutionRepoName, ConstString.Dependency.NodeType.REPOSITORY);
                result.AddOrUpdateEdge(solutionNode, solutionRepoNode, ConstString.Dependency.DependencyType.RESIDE);

                foreach (var proj in item.ProjectSet)
                {
                    string projecName = Path.GetFileNameWithoutExtension(proj.ProjectNodeId);
                    var projectAttributes = new Dictionary<string, string>()
                    {
                        [ConstString.Dependency.Attributes.RepositoryName] = proj.ProjectRepoName
                    };
                    var projectNode = result.AddOrUpdateNode(proj.ProjectNodeId, projecName, ConstString.Dependency.NodeType.PROJECT, projectAttributes);

                    var projectRepoNode = result.AddOrUpdateNode(proj.ProjectRepoName, proj.ProjectRepoName, ConstString.Dependency.NodeType.REPOSITORY);
                    result.AddOrUpdateEdge(projectNode, projectRepoNode, ConstString.Dependency.DependencyType.RESIDE);
                    result.AddOrUpdateEdge(solutionNode, projectNode, ConstString.Dependency.DependencyType.REFER);
                }
            }
            return result;
        }

        private DataTable GetProjectSolutionMappingTable(IEnumerable<(string SolutionRepoName, string SolutionNodeId, HashSet<(string ProjectRepoName, string ProjectNodeId)> ProjectSet)> mapping)
        {
            DataTable table = new DataTable("ProjectSolutionMapping");
            table.Columns.Add("SolutionRepository", typeof(string));
            table.Columns.Add("Solution", typeof(string));
            table.Columns.Add("ProjectRepository", typeof(string));
            table.Columns.Add("Project", typeof(string));
            foreach (var item in mapping)
            {
                foreach (var proj in item.ProjectSet)
                {
                    table.Rows.Add(item.SolutionRepoName, item.SolutionNodeId, proj.ProjectRepoName, proj.ProjectNodeId);
                }
            }
            return table;
        }

        private DirectedGraph GetProjectDependencies(Project p)
        {
            DirectedGraph result = new DirectedGraph();

            try
            {
                var docSummary = _projectAnalyzer.GetDocumentSummary(p.Documents);

                string projectNodeId = _utility.GetNodeIdProject(p);
                string repositoryName = _workspace.GetRepositoryName(p.FilePath);
                string projectTargetFramework = _projectAnalyzer.ParseDotNetVersion(p.FilePath);
                string projectType = _projectAnalyzer.GetProjectType(p);

                // Keep track of project attributes
                var projectAttributes = new Dictionary<string, string>();
                projectAttributes.Add(ConstString.Dependency.Attributes.RepositoryName, repositoryName);
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectClassCount, docSummary.NameTypeCount.ToString());
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectDataNameTypeCount, docSummary.DataOnlyNameTypeCount.ToString());
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectMethodCount, docSummary.MethodCount.ToString());
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectLinesOfCode, docSummary.LinesOfCode.ToString());
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectLanguage, p.Language);
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectTargetFramework, projectTargetFramework);
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectAssemblyName, p.AssemblyName ?? string.Empty);
                projectAttributes.Add(ConstString.Dependency.Attributes.ProjectType, projectType);


                // Add current project node
                var currentProjectNode = result.AddOrUpdateNode(projectNodeId, p.Name, ConstString.Dependency.NodeType.PROJECT, projectAttributes);
                var currentrojectRepoNode = result.AddOrUpdateNode(repositoryName, repositoryName, ConstString.Dependency.NodeType.REPOSITORY);
                result.AddOrUpdateEdge(currentProjectNode, currentrojectRepoNode, ConstString.Dependency.DependencyType.RESIDE);

                // 1. Analyze project references
                foreach (var projectRef in p.ProjectReferences)
                {
                    var referencedProject = p.Solution.GetProject(projectRef.ProjectId);
                    if (referencedProject != null)
                    {
                        var projectRefNodeId = _utility.GetNodeIdProject(referencedProject);
                        var projectRefNodeName = referencedProject.Name;

                        var targetNode = result.AddOrUpdateNode(projectRefNodeId, projectRefNodeName, ConstString.Dependency.NodeType.PROJECT);
                        var edge = result.AddOrUpdateEdge(currentProjectNode, targetNode, ConstString.Dependency.DependencyType.REFER);
                    }
                }

                // 2. Analyze metadata references (NuGet packages and assemblies)
                foreach (var metadataRef in p.MetadataReferences)
                {
                    if (metadataRef is PortableExecutableReference peRef && !string.IsNullOrEmpty(peRef.FilePath))
                    {
                        var assemblyInfo = _assemblyAnalyzer.GetAssemblyInfo(peRef.FilePath);
                        IReadOnlyDictionary<string, string> assemblyAttributes = new Dictionary<string, string>()
                        {
                            [ConstString.Dependency.Attributes.AssemblyName] = assemblyInfo.Name,
                            [ConstString.Dependency.Attributes.AssemblyVersion] = assemblyInfo.Version,
                            [ConstString.Dependency.Attributes.AssemblyTargetFramework] = assemblyInfo.TargetFramework,
                            [ConstString.Dependency.Attributes.AssemblyPath] = assemblyInfo.Path

                        };

                        var target = result.AddOrUpdateNode(assemblyInfo.Name, assemblyInfo.Name, ConstString.Dependency.NodeType.ASSEMBLY, assemblyAttributes);
                        result.AddOrUpdateEdge(currentProjectNode, target, ConstString.Dependency.DependencyType.REFER);

                    }
                }

                // 3. Analyze code-level dependencies
                var compilation = p.GetCompilationAsync().Result;
                if (compilation != null)
                {
                    AnalyzeCodeDependencies(p, compilation, result, docSummary.NameTypeInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error analyzing dependencies for project {ProjectName}: {ErrorMessage}", p.Name, ex.Message);
            }

            return result;
        }


        private void AnalyzeCodeDependencies(Project project, Compilation compilation, DirectedGraph graph, Dictionary<string, NameTypeSummary> nameTypeInfoSet)
        {
            try
            {
                var projectNode= graph.GetNode(_utility.GetNodeIdProject(project));
                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);
                    var root = syntaxTree.GetRoot();

                    // Find all "Significant" type declarations in this syntax tree. Although we have other types like delegates, dynamics, pointers etc, we will focus on main types
                    // to reduce noise in the dependency graph
                    var typeDeclarations = root.DescendantNodes()
                        .Where(node => node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax ||
                                      node is Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax ||
                                      node is Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax ||
                                      node is Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax);


                    foreach (var typeDecl in typeDeclarations)
                    {
                        var currentTypeSymbol = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;

                        // IsImplicitlyDeclared make sure we analyze only user-defined types, not compiler-generated ones
                        if (currentTypeSymbol != null && !currentTypeSymbol.IsImplicitlyDeclared)
                        {
                            var typeFullName = currentTypeSymbol.ToDisplayString();
                            var typeName = currentTypeSymbol.Name;

                
                            var currentTypeNode = graph.AddOrUpdateNode(typeFullName, typeName, GetNodeTypeFromSymbol(currentTypeSymbol));

                            var ls = typeDecl.GetLocation().GetLineSpan();
                            int[] locationTuple = [ls.StartLinePosition.Line, ls.StartLinePosition.Character, ls.EndLinePosition.Line, ls.EndLinePosition.Character];
                            string locationInfo = JsonConvert.SerializeObject(locationTuple);

                            currentTypeNode.AddOrUpdateAttribute(ConstString.Dependency.Attributes.NameTypePosition, locationInfo);
                            if (!string.IsNullOrEmpty(ls.Path))
                            {
                                currentTypeNode.AddOrUpdateAttribute(ConstString.Dependency.Attributes.ResourceFilePath, _workspace.GetResourceName(ls.Path));
                            }


                            if (nameTypeInfoSet.ContainsKey(typeFullName))
                            {
                                var summary = nameTypeInfoSet[typeFullName];
                                currentTypeNode.AddOrUpdateAttribute(ConstString.Dependency.Attributes.NameTypeMethodCount, summary.MethodCount.ToString());
                                currentTypeNode.AddOrUpdateAttribute(ConstString.Dependency.Attributes.NameTypeLinesOfCode, summary.LinesOfCode.ToString());
                            }

                            var residingAssembly = graph.AddOrUpdateNode(currentTypeSymbol.ContainingAssembly.Name, currentTypeSymbol.ContainingAssembly.Name, ConstString.Dependency.NodeType.ASSEMBLY);
                            graph.AddOrUpdateEdge(currentTypeNode, residingAssembly, ConstString.Dependency.DependencyType.RESIDE);
                            graph.AddOrUpdateEdge(currentTypeNode, projectNode, ConstString.Dependency.DependencyType.RESIDE);

                            UpdateBaseTypeDependencies(currentTypeNode, graph, currentTypeSymbol);
                            UpdateInterfaceDependencies(currentTypeNode, graph, currentTypeSymbol);
                            UpdateDataMembersDependencies(currentTypeNode, graph, currentTypeSymbol);
                            UpdateEventDependencies(currentTypeNode, graph, currentTypeSymbol);



                            // Analyze method call and property access dependencies
                            UpdateMethodDependencies(currentTypeNode, graph, currentTypeSymbol, typeDecl, semanticModel, nameTypeInfoSet);

                            // Analyze method dependencies
                            foreach (var method in currentTypeSymbol.GetMembers().OfType<IMethodSymbol>())
                            {
                                // Return type dependency
                                if (method.ReturnType != null && !IsBuiltInType(method.ReturnType))
                                {
                                    var returnTypeFullName = method.ReturnType.ToDisplayString();
                                    var returnTypeName = method.ReturnType.Name;
                                    var returnTypeNode = graph.AddOrUpdateNode(returnTypeFullName, returnTypeName, GetNodeTypeFromSymbol(method.ReturnType));
                                    var edge = graph.AddOrUpdateEdge(currentTypeNode, returnTypeNode, ConstString.Dependency.DependencyType.USE);
                                }

                                // Parameter type dependencies
                                foreach (var parameter in method.Parameters)
                                {
                                    if (!IsBuiltInType(parameter.Type))
                                    {
                                        var paramTypeFullName = parameter.Type.ToDisplayString();
                                        var paramTypeName = parameter.Type.Name;
                                        var paramTypeNode = graph.AddOrUpdateNode(paramTypeFullName, paramTypeName, GetNodeTypeFromSymbol(parameter.Type));
                                        var edge = graph.AddOrUpdateEdge(currentTypeNode, paramTypeNode, ConstString.Dependency.DependencyType.USE);

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error analyzing code dependencies for project {ProjectName}: {ErrorMessage}", project.Name, ex.Message);
            }
        }

        private void UpdateBaseTypeDependencies(INode typeNode, DirectedGraph graph, INamedTypeSymbol typeSymbol)
        {
            // Base types, since every inheritance has System.Object as base type, we skip that to avoid noise
            if (typeSymbol.BaseType != null && typeSymbol.BaseType.SpecialType != SpecialType.System_Object)
            {
                var baseTypeSymbol = GetOriginalDefintion(typeSymbol.BaseType);
                var baseStypeFullName = baseTypeSymbol.ToDisplayString();
                var baseTypeName = baseTypeSymbol.Name;
                var baseTypeNode = graph.AddOrUpdateNode(baseStypeFullName, baseTypeName, GetNodeTypeFromSymbol(baseTypeSymbol));
                graph.AddOrUpdateEdge(typeNode, baseTypeNode, ConstString.Dependency.DependencyType.INHERIT);
                var baseTypeAssembly = graph.AddOrUpdateNode(baseTypeSymbol.ContainingAssembly.Name, baseTypeSymbol.ContainingAssembly.Name, ConstString.Dependency.NodeType.ASSEMBLY);
                graph.AddOrUpdateEdge(baseTypeNode, baseTypeAssembly, ConstString.Dependency.DependencyType.RESIDE);
                graph.AddOrUpdateEdge(typeNode, baseTypeAssembly, ConstString.Dependency.DependencyType.REFER);
                var allTypesUsed = ExtractAllTypeSymbols(typeSymbol.BaseType);
                foreach (var ts in allTypesUsed)
                {
                    //we ignore the base type itself as we have already added it
                    if (!SymbolEqualityComparer.Default.Equals(ts, baseTypeSymbol))
                    {
                        var usedTypeFullName = ts.ToDisplayString();
                        var usedTypeName = ts.Name;
                        var usedTypeNode = graph.AddOrUpdateNode(usedTypeFullName, usedTypeName, GetNodeTypeFromSymbol(ts));
                        graph.AddOrUpdateEdge(typeNode, usedTypeNode, ConstString.Dependency.DependencyType.USE);
                        var usedTypeAssembly = graph.AddOrUpdateNode(ts.ContainingAssembly.Name, ts.ContainingAssembly.Name, ConstString.Dependency.NodeType.ASSEMBLY);
                        graph.AddOrUpdateEdge(usedTypeNode, usedTypeAssembly, ConstString.Dependency.DependencyType.RESIDE);
                        graph.AddOrUpdateEdge(typeNode, usedTypeAssembly, ConstString.Dependency.DependencyType.REFER);
                    }
                }
            }
        }
        private void UpdateInterfaceDependencies(INode typeNode, DirectedGraph graph, INamedTypeSymbol typeSymbol)
        {
            // Interfaces implementation by a name type
            foreach (var interfaceType in typeSymbol.Interfaces)
            {
                var interfaceTypeSymbol = GetOriginalDefintion(interfaceType);
                var interfaceTypeFullName = interfaceTypeSymbol.ToDisplayString();
                var interfaceTypeName = interfaceTypeSymbol.Name;
                var interfaceNode = graph.AddOrUpdateNode(interfaceTypeFullName, interfaceTypeName, GetNodeTypeFromSymbol(interfaceType));
                graph.AddOrUpdateEdge(typeNode, interfaceNode, ConstString.Dependency.DependencyType.IMPLEMENT);
                var interfaceAssembly = graph.AddOrUpdateNode(interfaceTypeSymbol.ContainingAssembly.Name, interfaceTypeSymbol.ContainingAssembly.Name, ConstString.Dependency.NodeType.ASSEMBLY);
                graph.AddOrUpdateEdge(interfaceNode, interfaceAssembly, ConstString.Dependency.DependencyType.RESIDE);
                graph.AddOrUpdateEdge(typeNode, interfaceAssembly, ConstString.Dependency.DependencyType.REFER);
                var allTypesUsed = ExtractAllTypeSymbols(interfaceType);
                foreach (var ts in allTypesUsed)
                {
                    //we ignore the interface type itself as we have already added it
                    if (!SymbolEqualityComparer.Default.Equals(ts, interfaceTypeSymbol))
                    {
                        var usedTypeFullName = ts.ToDisplayString();
                        var usedTypeName = ts.Name;
                        var usedTypeNode = graph.AddOrUpdateNode(usedTypeFullName, usedTypeName, GetNodeTypeFromSymbol(ts));
                        var edge2 = graph.AddOrUpdateEdge(typeNode, usedTypeNode, ConstString.Dependency.DependencyType.USE);
                        var usedTypeAssembly = graph.AddOrUpdateNode(ts.ContainingAssembly.Name, ts.ContainingAssembly.Name, ConstString.Dependency.NodeType.ASSEMBLY);
                        graph.AddOrUpdateEdge(usedTypeNode, usedTypeAssembly, ConstString.Dependency.DependencyType.RESIDE);
                        graph.AddOrUpdateEdge(typeNode, usedTypeAssembly, ConstString.Dependency.DependencyType.REFER);
                    }
                }
            }
        }

        private void UpdateDataMembersDependencies(INode typeNode, DirectedGraph graph, INamedTypeSymbol typeSymbol)
        {
            // Data members (fields and properties)
            var memberSet = typeSymbol.GetMembers();
            var distictFieldAndPropertyTypes = memberSet.Where(m => m is IFieldSymbol || m is IPropertySymbol)
                .Select(static s =>
                {
                    ITypeSymbol typeSymbol1 = s switch
                    {
                        IFieldSymbol field => field.Type,
                        IPropertySymbol property => property.Type
                    };
                    return typeSymbol1;
                }
                ).DistinctBy(ts => ts.ToDisplayString()).ToList();
            foreach (var memberType in distictFieldAndPropertyTypes)
            {
                if (!IsBuiltInType(memberType))
                {
                    var allTypeSymbolsUsed = ExtractAllTypeSymbols(memberType);
                    foreach (var ts in allTypeSymbolsUsed)
                    {
                        var memberTypeFullName = ts.ToDisplayString();
                        var memberTypeName = ts.Name;
                        var memberTypeNode = graph.AddOrUpdateNode(memberTypeFullName, memberTypeName, GetNodeTypeFromSymbol(ts));
                        var edge2 = graph.AddOrUpdateEdge(typeNode, memberTypeNode, ConstString.Dependency.DependencyType.COMPOSITION);
                        var memberTypeAssembly = graph.AddOrUpdateNode(ts.ContainingAssembly.Name, ts.ContainingAssembly.Name, ConstString.Dependency.NodeType.ASSEMBLY);
                        graph.AddOrUpdateEdge(memberTypeNode, memberTypeAssembly, ConstString.Dependency.DependencyType.RESIDE);
                        graph.AddOrUpdateEdge(typeNode, memberTypeAssembly, ConstString.Dependency.DependencyType.REFER);
                    }
                }
            }
        }

        private void UpdateEventDependencies(INode typeNode, DirectedGraph graph, INamedTypeSymbol typeSymbol)
        {
            // Events
            foreach (var eventSymbol in typeSymbol.GetMembers().OfType<IEventSymbol>())
            {
                var eventType = eventSymbol.Type;
                if (eventType != null && !IsBuiltInType(eventType))
                {
                    var eventTypeFullName = eventType.ToDisplayString();
                    var eventTypeName = eventType.Name;
                    var eventTypeNode = graph.AddOrUpdateNode(eventTypeFullName, eventTypeName, GetNodeTypeFromSymbol(eventType));
                    graph.AddOrUpdateEdge(typeNode, eventTypeNode, ConstString.Dependency.DependencyType.USE);
                    var eventTypeAssembly = graph.AddOrUpdateNode(eventType.ContainingAssembly.Name, eventType.ContainingAssembly.Name, ConstString.Dependency.NodeType.ASSEMBLY);
                    graph.AddOrUpdateEdge(eventTypeNode, eventTypeAssembly, ConstString.Dependency.DependencyType.RESIDE);
                    graph.AddOrUpdateEdge(typeNode, eventTypeAssembly, ConstString.Dependency.DependencyType.REFER);
                }
            }
        }

        private void UpdateMethodDependencies(INode typeNode, DirectedGraph graph, INamedTypeSymbol typeSymbol, SyntaxNode typeDecl, SemanticModel semanticModel, Dictionary<string, NameTypeSummary> nameTypeInfoSet)
        {
            // create a lookup table for method calls and property accesses
            Dictionary<string, MethodSummary> methodInfoSet = new Dictionary<string, MethodSummary>();
            foreach (var nameTypeInfo in nameTypeInfoSet)
            {
                foreach (var method in nameTypeInfo.Value.MethodInfo)
                {
                    string methodKey = $"{nameTypeInfo.Key}.{method.Key}";
                    if (!methodInfoSet.ContainsKey(methodKey))
                    {
                        methodInfoSet[methodKey] = new MethodSummary
                        {
                            LinesOfCode = method.Value.LinesOfCode,
                            NumberOfParameters = method.Value.NumberOfParameters
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Duplicate method key found: {MethodKey} in class {ClassName}. This may indicate an issue with the class summary data.", methodKey, nameTypeInfo.Key);
                    }
                }
            }


            try
            {
                // Find all method invocations within this type
                var invocations = typeDecl.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>();

                foreach (var invocation in invocations)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(invocation);


                    if (symbolInfo.Symbol != null && symbolInfo.Symbol is IMethodSymbol methodSymbol)
                    {
                        var containingType = methodSymbol.ContainingType;
                        if (containingType != null &&
                            !IsBuiltInType(containingType) &&
                            !SymbolEqualityComparer.Default.Equals(containingType, typeSymbol)) // Don't create self-dependencies
                        {
                            var targetTypeFullName = containingType.ToDisplayString();
                            var targetTypeName = containingType.Name;
                            var targetTypeNode = graph.AddOrUpdateNode(targetTypeFullName, targetTypeName, GetNodeTypeFromSymbol(containingType));
                            var edge = graph.AddOrUpdateEdge(typeNode, targetTypeNode, ConstString.Dependency.DependencyType.INVOKE);

                            string methodKey = $"{targetTypeFullName}.{methodSymbol.Name}";
                            var targetMethodNode = graph.AddOrUpdateNode(methodKey, methodSymbol.Name, ConstString.Dependency.NodeType.METHOD);
                            graph.AddOrUpdateEdge(typeNode, targetMethodNode, ConstString.Dependency.DependencyType.INVOKE);
                            graph.AddOrUpdateEdge(targetTypeNode, targetMethodNode, ConstString.Dependency.DependencyType.HAS);

                            var allReturnTypesUsed= ExtractAllTypeSymbols(methodSymbol.ReturnType) 
                                .Where(t => !IsBuiltInType(t) && !SymbolEqualityComparer.Default.Equals(t, containingType));
                            foreach(var ts in allReturnTypesUsed)
                            {
                                var returnTypeFullName = ts.ToDisplayString();
                                var returnTypeName = ts.Name;
                                var returnTypeNode = graph.AddOrUpdateNode(returnTypeFullName, returnTypeName, GetNodeTypeFromSymbol(ts));
                                graph.AddOrUpdateEdge(targetMethodNode, returnTypeNode, ConstString.Dependency.DependencyType.USE);
                                graph.AddOrUpdateEdge(typeNode, returnTypeNode, ConstString.Dependency.DependencyType.USE);
                            }

                            methodSymbol.Parameters.ToList().ForEach(param =>
                            {
                                var allParamTypesUsed = ExtractAllTypeSymbols(param.Type)
                                    .Where(t => !IsBuiltInType(t) && !SymbolEqualityComparer.Default.Equals(t, containingType));
                                foreach (var ts in allParamTypesUsed)
                                {
                                    //add parameter type dependencies
                                    var paramTypeFullName = ts.ToDisplayString();
                                    var paramTypeName = ts.Name;
                                    var paramTypeNode = graph.AddOrUpdateNode(paramTypeFullName, paramTypeName, GetNodeTypeFromSymbol(ts));
                                    graph.AddOrUpdateEdge(targetMethodNode, paramTypeNode, ConstString.Dependency.DependencyType.USE);
                                    graph.AddOrUpdateEdge(typeNode, paramTypeNode, ConstString.Dependency.DependencyType.USE);
                                }
                            });

                        }
                    }
                    else
                    {
                        var nodeType=GetNodeTypeFromSymbol(symbolInfo.Symbol);
                        var blackHoleNode=graph.AddOrUpdateNode(nodeType, nodeType, ConstString.Dependency.NodeType.AMBIGUOUS);
                        graph.AddOrUpdateEdge(typeNode, blackHoleNode, ConstString.Dependency.DependencyType.INVOKE);

                    }
                }

                // Find all member access expressions (property/field access)
                var memberAccesses = typeDecl.DescendantNodes().Where(node =>
         node is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax ||
         node is Microsoft.CodeAnalysis.CSharp.Syntax.ElementAccessExpressionSyntax ||
         node is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax);


                foreach (var memberAccess in memberAccesses)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
                    if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
                    {
                        var containingType = fieldSymbol.ContainingType;
                        if (containingType != null &&
                            !IsBuiltInType(containingType) &&
                            !SymbolEqualityComparer.Default.Equals(containingType, typeSymbol))
                        {
                            var targetTypeFullName = containingType.ToDisplayString();
                            var targetTypeName = containingType.Name;
                            var targetTypeNode = graph.AddOrUpdateNode(targetTypeFullName, targetTypeName, GetNodeTypeFromSymbol(containingType));
                            var edge = graph.AddOrUpdateEdge(typeNode, targetTypeNode, ConstString.Dependency.DependencyType.ACCESS);
                        }
                    }
                    else if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
                    {
                        var containingType = propertySymbol.ContainingType;
                        if (containingType != null &&
                            !IsBuiltInType(containingType) &&
                            !SymbolEqualityComparer.Default.Equals(containingType, typeSymbol))
                        {
                            var targetTypeFullName = containingType.ToDisplayString();
                            var targetTypeName = containingType.Name;
                            var targetTypeNode = graph.AddOrUpdateNode(targetTypeFullName, targetTypeName, GetNodeTypeFromSymbol(containingType));
                            var edge = graph.AddOrUpdateEdge(typeNode, targetTypeNode, ConstString.Dependency.DependencyType.ACCESS);
                        }
                    }else if (symbolInfo.Symbol is IEventSymbol eventSymbol)
                    {
                        var containingType = eventSymbol.ContainingType;
                        if (containingType != null &&
                            !IsBuiltInType(containingType) &&
                            !SymbolEqualityComparer.Default.Equals(containingType, typeSymbol))
                        {
                            var targetTypeFullName = containingType.ToDisplayString();
                            var targetTypeName = containingType.Name;
                            var targetTypeNode = graph.AddOrUpdateNode(targetTypeFullName, targetTypeName, GetNodeTypeFromSymbol(containingType));
                            var edge = graph.AddOrUpdateEdge(typeNode, targetTypeNode, ConstString.Dependency.DependencyType.ACCESS);
                        }
                    }
                }

                // Find object creation expressions (new keyword)
                var objectCreations = typeDecl.DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax>();

                foreach (var objectCreation in objectCreations)
                {
                    var symbolInfo = semanticModel.GetSymbolInfo(objectCreation);
                    if (symbolInfo.Symbol is IMethodSymbol constructorSymbol)
                    {
                        var containingType = constructorSymbol.ContainingType;
                        if (containingType != null &&
                            !IsBuiltInType(containingType) &&
                            !SymbolEqualityComparer.Default.Equals(containingType, typeSymbol))
                        {
                            var targetTypeFullName = containingType.ToDisplayString();
                            var targetTypeName = containingType.Name;
                            var targetTypeNode = graph.AddOrUpdateNode(targetTypeFullName, targetTypeName, GetNodeTypeFromSymbol(containingType));

                            var edge = graph.AddOrUpdateEdge(typeNode, targetTypeNode, ConstString.Dependency.DependencyType.CREATE);
                        }
                    }
                }

                // Find local variable declarations that might indicate dependencies
                var variableDeclarations = typeDecl.DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax>();

                foreach (var varDecl in variableDeclarations)
                {
                    var typeInfo = semanticModel.GetTypeInfo(varDecl.Type);
                    if (typeInfo.Type != null &&
                        !IsBuiltInType(typeInfo.Type) &&
                        !SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeSymbol))
                    {
                        var targetTypeFullName = typeInfo.Type.ToDisplayString();
                        var targetTypeName = typeInfo.Type.Name;
                        var targetTypeNode = graph.AddOrUpdateNode(targetTypeFullName, targetTypeName, GetNodeTypeFromSymbol(typeInfo.Type));
                        var edge = graph.AddOrUpdateEdge(typeNode, targetTypeNode, ConstString.Dependency.DependencyType.DECLARE);

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error analyzing method call dependencies for type {TypeName}: {ErrorMessage}", typeSymbol.Name, ex.Message);
            }
        }


        /// <summary>
        /// Determines the appropriate node type string from a type symbol
        /// </summary>
        /// <param name="typeSymbol">The type symbol to analyze</param>
        /// <returns>The corresponding node type constant from ConstString.Dependency.NodeType</returns>
        private string GetNodeTypeFromSymbol(ISymbol typeSymbol)
        {
            // Handle different kinds of type symbols
            switch (typeSymbol)
            {
                case IArrayTypeSymbol arrayType:
                    // For arrays like string[], List<int>[], etc., get the element type
                    return ConstString.Dependency.NodeType.ARRAY;

                case IMethodSymbol methodType:
                    return ConstString.Dependency.NodeType.METHOD;

                case IAssemblySymbol assemblyType:
                    return ConstString.Dependency.NodeType.ASSEMBLY;

                case IPointerTypeSymbol pointerType:
                    // For pointer types like int*, get the pointed-at type
                    return ConstString.Dependency.NodeType.MISCELLANEOUS;

                case null:
                    return ConstString.Dependency.NodeType.AMBIGUOUS;

                case IErrorTypeSymbol errorType:
                    // For error types (unresolved types), treat as ambiguous
                    return ConstString.Dependency.NodeType.AMBIGUOUS;
                case IDynamicTypeSymbol dynamicType:
                    // For dynamic types, treat as miscellaneous
                    return ConstString.Dependency.NodeType.MISCELLANEOUS;

                case IAliasSymbol aliasSymbol:
                    // For alias symbols, analyze the target type
                    return GetNodeTypeFromSymbol(aliasSymbol.Target);

           

                case INamedTypeSymbol namedType:
                    // Handle generic types by checking if they have type arguments
                    if (namedType.IsGenericType && namedType.TypeArguments.Length > 0)
                    {
                        return ConstString.Dependency.NodeType.GENERIC;
                    }
                    else
                    {
                        // Regular named types (non-generic)
                        return namedType.TypeKind switch
                        {
                            TypeKind.Class => ConstString.Dependency.NodeType.CLASS,
                            TypeKind.Interface => ConstString.Dependency.NodeType.INTERFACE,
                            TypeKind.Enum => ConstString.Dependency.NodeType.ENUM,
                            TypeKind.Struct => ConstString.Dependency.NodeType.STRUCT,
                            TypeKind.Delegate => ConstString.Dependency.NodeType.DELEGATE,
                            _ => ConstString.Dependency.NodeType.MISCELLANEOUS // Default fallback
                        };
                    }

                case IFunctionPointerTypeSymbol functionPointer:
                    // For function pointers (C# 9+), treat as delegate-like
                    return ConstString.Dependency.NodeType.DELEGATE;

                case ITypeParameterSymbol typeParameter:
                    // For type parameters like T in class MyClass<T>, treat as miscellaneous
                    return ConstString.Dependency.NodeType.GENERIC;

                default:
                    // For any other type symbols, use class as default
                    return ConstString.Dependency.NodeType.MISCELLANEOUS;
            }
        }

        /// <summary>
        /// Extracts all meaningful type symbols from a complex type (arrays, generics, etc.)
        /// This is useful for dependency analysis where we want to track all types involved
        /// </summary>
        /// <param name="typeSymbol">The type symbol to analyze</param>
        /// <returns>A collection of all meaningful type symbols found</returns>
        private IEnumerable<ITypeSymbol> ExtractAllTypeSymbols(ITypeSymbol typeSymbol)
        {
            var result = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var typeStack = new Stack<ITypeSymbol>();

            // Start with the initial type symbol
            if (typeSymbol != null)
            {
                typeStack.Push(typeSymbol);
            }

            // Process types iteratively using a stack
            while (typeStack.Count > 0)
            {
                var currentType = typeStack.Pop();

                // Skip if null or already processed
                if (currentType == null || result.Contains(currentType))
                    continue;

                // Add the current type if it's meaningful (not built-in)
                if (!IsBuiltInTypeSingle(currentType))
                {
                    var typeToUse = GetOriginalDefintion(currentType);
                    result.Add(typeToUse);
                }

                // Add nested types to the stack for processing
                switch (currentType)
                {
                    case IArrayTypeSymbol arrayType:
                        typeStack.Push(arrayType.ElementType);
                        break;

                    case IPointerTypeSymbol pointerType:
                        typeStack.Push(pointerType.PointedAtType);
                        break;

                    case INamedTypeSymbol namedType when namedType.IsGenericType:
                        // For generics, add all type arguments to the stack
                        foreach (var typeArg in namedType.TypeArguments)
                        {
                            typeStack.Push(typeArg);
                        }
                        break;

                    case IFunctionPointerTypeSymbol functionPointer:
                        typeStack.Push(functionPointer.Signature.ReturnType);
                        foreach (var param in functionPointer.Signature.Parameters)
                        {
                            typeStack.Push(param.Type);
                        }
                        break;
                }
            }

            return result;
        }

        private ITypeSymbol GetOriginalDefintion(ITypeSymbol typeSymbol)
        {
            ITypeSymbol result = typeSymbol;

            // If it's a constructed generic type (like Person<Address, Vehicle>), 
            // get the original generic type definition (like Person<T1, T2>)
            if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType && !namedType.IsUnboundGenericType)
            {
                // Get the original generic type definition
                result = namedType.OriginalDefinition;
                // Alternative: you can also use namedType.ConstructedFrom which should give the same result
                // typeToUse = namedType.ConstructedFrom;
            }
            // If it's an array type (like string[]), get the element type (like string)
            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                result = arrayType.ElementType;
            }

            return result;
        }
        private bool IsBuiltInType(ITypeSymbol typeSymbol)
        {
            // Extract all nested type symbols from the complex type structure
            var allNestedTypes = ExtractAllTypeSymbols(typeSymbol);

            // If any nested type is found (meaning it's not built-in), return false
            // The ExtractAllTypeSymbols method only adds non-built-in types to the result
            return !allNestedTypes.Any();
        }
        /// <summary>
        /// Checks if a type symbol is built-in based on configuration rules.
        /// This is a helper method used by ExtractAllTypeSymbols.
        /// </summary>
        /// <param name="typeSymbol">The type symbol to check</param>
        /// <returns>True if the type is built-in, false otherwise</returns>
        private bool IsBuiltInTypeSingle(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return true;

            // Check if this specific type is a built-in type
            if (typeSymbol.SpecialType != SpecialType.None)
            {
                return true; // Built-in .NET types like int, string, etc.
            }

            if( typeSymbol is IArrayTypeSymbol arrayType)
            {
                return false;
            }

            string fullName = typeSymbol.ToDisplayString();

            // We always include Namespaces marked as to be included - these are considered non-built-in
            if (_config.NamespacePrefixToInclude != null && _config.NamespacePrefixToInclude.Any(ns => fullName.StartsWith(ns)))
            {
                return false; // Non-built-in type found
            }

            // Types marked for exclusion are considered built-in
            if (_config.NamespacePrefixToExclude != null && _config.NamespacePrefixToExclude.Any(ns => fullName.StartsWith(ns)))
            {
                return true; // Considered built-in
            }

            // If not explicitly included or excluded, consider non built-in by default
            return false;
        }
    }
}
