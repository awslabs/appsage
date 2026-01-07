using AppSage.Core.ComplexType.Graph;
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
        static IDirectedGraph _graph = null;
        static IDirectedGraph _filteredGraph = null;

        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        IMetricReader _metricReader;
        IDynamicCompiler _compiler;
        public CodeGraphGenerator(IAppSageLogger logger, IDynamicCompiler compiler, IAppSageWorkspace workspace, IMetricReader metricReader)
        {
            _logger = logger;
            _compiler = compiler;
            _workspace = workspace;
            _metricReader = metricReader;
        }

        [McpServerTool, Description("Run the code against the code graph and generate data table")]
        public IEnumerable<DataTable> ExecuteTableQuery(string codeToCompileAndRun)
        {
            if (string.IsNullOrEmpty(codeToCompileAndRun))
            {
                _logger.LogError("No code is provided to execute against the graph.");
            }

            EnsureGraph();

            var result = _compiler.CompileAndExecute<IEnumerable<DataTable>>(codeToCompileAndRun, _graph);
            return result;
        }

        [McpServerTool, Description("Run the code against the code graph and generate a directed graph")]
        public DirectedGraph ExecuteGraphQuery(string codeToCompileAndRun)
        {
            if (string.IsNullOrEmpty(codeToCompileAndRun))
            {
                _logger.LogError("No code is provided to execute against the graph.");
            }

            EnsureGraph();

            var result = _compiler.CompileAndExecute<DirectedGraph>(codeToCompileAndRun, _graph);
            return result;
        }


        [McpServerTool, Description("Get the current appsage workspace folder where the data are loaded from")]
        public string GetWorkspaceRootFolder()
        {
            return _workspace.RootFolder;
        }

        [McpServerTool, Description("Load the data and refresh the current graph metrics. Usually done once, unless explicity asked to do so.")]
        public string LoadGraphData()
        {
            _logger.LogInformation("Loading graph data from the metric store.");

            var metrics = _metricReader.GetMetricSet()
                .AsParallel().WithDegreeOfParallelism(10).Where(m => m is IMetricValue<DirectedGraph>)
                .Cast<IMetricValue<DirectedGraph>>();

            _logger.LogInformation($"Found {metrics.Count()} graph metrics in the metric store.");

            var graphSet = metrics.Where(x => x is IMetricValue<DirectedGraph>)
                .Cast<IMetricValue<DirectedGraph>>().Select(r => r.Value);

            _logger.LogInformation("Merging multiple graphs to form a one.");

            _graph = DirectedGraph.MergeGraph(graphSet);

            //initially the filtered graph is the same as the full graph
            _filteredGraph = _graph;
            _logger.LogInformation("Loading completed.");

            if (_graph == null)
            {
                return $"No graph data is found in '{_workspace.ProviderOutputFolder}'";
            }
            return $"Graph data is loaded from '{_workspace.ProviderOutputFolder}'. Found {_graph.Nodes.Count} nodes and {_graph.Edges.Count} edges";
        }


        [McpServerTool, Description("Filter the node types and edge types. The values are case sensitive.")]
        public string FilterGraphData(string[] nodeTypes, string[] edgeTypes)
        {
            EnsureGraph();

            var filteredGraph = new DirectedGraph();

            // Add nodes that match the specified types
            if (nodeTypes != null && nodeTypes.Length > 0)
            {
                foreach (var node in _graph.Nodes.Where(n => nodeTypes.Contains(n.Type)))
                {
                    filteredGraph.AddOrUpdateNode(node);
                }
            }
            else
            {
                // If no node types specified, include all nodes
                foreach (var node in _graph.Nodes)
                {
                    filteredGraph.AddOrUpdateNode(node);
                }
            }

            // Add edges that match the specified types and connect included nodes
            if (edgeTypes != null && edgeTypes.Length > 0)
            {
                foreach (var edge in _graph.Edges.Where(e => edgeTypes.Contains(e.Type)))
                {
                    // Only add edge if both source and target nodes are in the filtered graph
                    if (filteredGraph.ContainsNode(edge.Source) && filteredGraph.ContainsNode(edge.Target))
                    {
                        filteredGraph.AddOrUpdateEdge(edge);
                    }
                }
            }
            else
            {
                // If no edge types specified, include all edges between included nodes
                foreach (var edge in _graph.Edges)
                {
                    if (filteredGraph.ContainsNode(edge.Source) && filteredGraph.ContainsNode(edge.Target))
                    {
                        filteredGraph.AddOrUpdateEdge(edge);
                    }
                }
            }

            _filteredGraph = filteredGraph;
            return $"Graph data is filtered. Original graph had [{_graph.Nodes.Count} nodes and {_graph.Edges.Count} edges]. New filtered graph has [{_filteredGraph.Nodes.Count} nodes and {_filteredGraph.Edges.Count} edges] ";
        }
        private void EnsureGraph()
        {
            if (_graph == null)
            {
                lock (_padlock)
                {
                    if (_graph == null)
                    {
                        LoadGraphData();
                    }
                }
            }
        }
    }
}
