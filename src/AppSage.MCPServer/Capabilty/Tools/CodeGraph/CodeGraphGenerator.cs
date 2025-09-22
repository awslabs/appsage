using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using DocumentFormat.OpenXml.Wordprocessing;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Data;

namespace AppSage.MCPServer.Capabilty.Tools.CodeGraph
{
    [McpServerToolType]
    [CapabilityRegistration("CodeGraph", @"Tools\CodeGraph")]
    public class CodeGraphGenerator
    {
        static object _padlock = new object();
        static DirectedGraph _graph = null;

        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        IMetricReader _metricReader;
        IDynamicCompiler _compiler;
        public CodeGraphGenerator(IAppSageLogger logger,IDynamicCompiler compiler,IAppSageWorkspace workspace,IMetricReader metricReader)
        {
            _logger = logger;
            _compiler = compiler;
            _workspace = workspace;
            _metricReader = metricReader;
        }
        [McpServerTool, Description("Run the code against the code graph and generate data table")]
        public IEnumerable<DataTable> ExecuteTableQuery(string codeToCompileAndRun) {
            if (string.IsNullOrEmpty(codeToCompileAndRun)) {
                _logger.LogError("No code is provided to execute against the graph.");
            }

            if (_graph == null) { 
                lock(_padlock) {
                    if (_graph == null) { 
                        LoadGraphData();
                    }
                }
            }
            var result= _compiler.CompileAndExecute<IEnumerable<DataTable>>(codeToCompileAndRun, _graph);
            return result;
        }


        [McpServerTool, Description("Get the current appsage workspace folder where the data are loaded from")]
        public string GetWorkspaceRootFolder()
        {
            return _workspace.RootFolder;
        }

        [McpServerTool, Description("Load the data and refresh the current graph metrics.")]
        public string LoadGraphData()
        {
            _logger.LogInformation("Loading graph data from the metric store.");
            var metrics = _metricReader.GetMetricSet();
            string providerName = "AppSage.Providers.DotNet.DependencyAnalysis.DotNetDependencyAnalysisProvider";

            var projectDependencies = metrics
                .Where(x => x.Provider == providerName)
                .Where(x => x.Name == MetricName.DotNet.Project.CODE_DEPENDENCY_GRAPH)
                .Cast<IResourceMetricValue<DirectedGraph>>().Select(r => r.Value);

            _graph= DirectedGraph.MergeGraph(projectDependencies);
            _logger.LogInformation("Loading completed.");

            if (_graph == null)
            {
                return $"No graph data is found in '{_workspace.ProviderOutputFolder}'";
            }
            return $"Graph data is loaded from '{_workspace.ProviderOutputFolder}'. Found {_graph.Nodes.Count} nodes and {_graph.Edges.Count} edges";
        }

       
    }
}
