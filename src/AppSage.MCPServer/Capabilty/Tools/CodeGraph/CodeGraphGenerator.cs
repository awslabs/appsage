using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
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
        static string _currentGraphLocation = null;

        DirectedGraph _graph = null;

        IAppSageLogger _logger;
        IDynamicCompiler _compiler;
        public CodeGraphGenerator(IAppSageLogger logger,IDynamicCompiler compiler)
        {
            _currentGraphLocation = @"C:\Dev\SampleAppSageWorkspace\Output\LastRun";
            _logger = logger;
            _compiler = compiler;
        }
        [McpServerTool, Description("Run the code against the graph and generate data table")]
        public IEnumerable<DataTable> ExecuteTableQuery(string codeToCompileAndRun) {
            if (string.IsNullOrEmpty(codeToCompileAndRun)) { 
                throw new ArgumentNullException("The code cannot be empty");
            }
            if (_graph == null) { 
                LoadGraphData();
            }
            var result= _compiler.CompileAndExecute<IEnumerable<DataTable>>(codeToCompileAndRun, _graph);
            return result;
        }

   


        [McpServerTool, Description("Get the folder path where the current code graph data will be loaded from")]
        public string GetCurrentGraphLocation()
        {
            if (string.IsNullOrEmpty(_currentGraphLocation))
            {
                return "No graph location is found";
            }
            if (!System.IO.Directory.Exists(_currentGraphLocation))
            {
                return $"The current graph location '{_currentGraphLocation}' does not exist";
            }
            return _currentGraphLocation;
        }
        [McpServerTool, Description("Set the folder path where the code graph data will be loaded from.")]
        public string SetCurrentGraphLocation(
            [Description("Valid forlder path that points to graph data. The directory should exits.")]string location
            )
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentNullException("The location cannot be empty");
            }
            if (!System.IO.Directory.Exists(location))
            {
                throw new ArgumentNullException($"The location '{location}' does not exist");
            }
            _currentGraphLocation = location;
            return $"The current graph location is set to '{_currentGraphLocation}'";
        }
        [McpServerTool, Description("Load the graph data from the graph location. Make sure you have first called set current graph location first to set the graph data path")]
        public string LoadGraphData()
        {

            if (string.IsNullOrEmpty(_currentGraphLocation))
            {
                throw new ArgumentNullException("The current code graph location is not properly set. Set the current graph location first");
            }
            if (!System.IO.Directory.Exists(_currentGraphLocation))
            {
                throw new ArgumentNullException($"The location {_currentGraphLocation} does not exist. Ensure you set the curretn graph location to a valid directory");
            }

            _graph = LoadGraphData(_currentGraphLocation);
            if(_graph == null)
            {
                return $"No graph data is found in '{_currentGraphLocation}'";
            }
            return $"Graph data is loaded from '{_currentGraphLocation}'. Found {_graph.Nodes.Count} nodes and {_graph.Edges.Count} edges";
        }

        private DirectedGraph LoadGraphData(string metricFolder)
        {
            var metrics = GetAllMetrics(metricFolder);
            string providerName = "AppSage.Providers.DotNet.DependencyAnalysis.DotNetDependencyAnalysisProvider";

            var projectDependencies = metrics
                .Where(x => x.Provider == providerName)
                .Where(x => x.Name == MetricName.DotNet.Project.CODE_DEPENDENCY_GRAPH)
                .Cast<IResourceMetricValue<DirectedGraph>>().Select(r=>r.Value);
        
            return DirectedGraph.MergeGraph(projectDependencies);
        }

        protected IEnumerable<IMetric> GetAllMetrics(string metricFolder )
        {
            if (!Directory.Exists(metricFolder))
            {
                throw new DirectoryNotFoundException($"The directory {metricFolder} does not exist.");
            }
            var fileSet = Directory.GetFiles(metricFolder, "*.json", System.IO.SearchOption.AllDirectories);



            List<IMetric> result = new List<IMetric>();
            foreach (var file in fileSet)
            {
                if (System.IO.File.Exists(file))
                {
                    string json = System.IO.File.ReadAllText(file);
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    var metrics = JsonConvert.DeserializeObject<IEnumerable<IMetric>>(json, settings);
                    result.AddRange(metrics);
                }
            }
            return result;
        }
    }
}
