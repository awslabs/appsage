using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Const;
using AppSage.Core.Metric;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace AppSage.MCPServer
{
    public class QueryTool
    {
        private static readonly object _padLock = new object();
        static DirectedGraph _source;
        
        internal static string LoadGraph(string path)
        {
            var mergedGraph = LoadGraphInternal(path);
            _source = mergedGraph;

            string content = File.ReadAllText(@"C:\Dev\AmazonCode\AppSage\src\AppSage.MCPServer\SampleQuery\SampleGraphFilter3.cs");
         

            string result = $"Loaded the graph with Nodes:{mergedGraph.Nodes.Count}, Edges:{mergedGraph.Edges.Count}";
            return result;
        }

        /// <summary>
        /// Loads the graph data from the specified path and returns the merged graph
        /// </summary>
        /// <param name="path">Path to the metrics data</param>
        /// <returns>Merged DirectedGraph containing all the loaded data</returns>
        internal static DirectedGraph LoadGraphInternal(string path)
        {
            var allMetrics = GetAllMetrics(path);
            string providerName = "AppSage.Providers.DotNet.DependencyAnalysis.DotNetDependencyAnalysisProvider";

            var projectDependencies = allMetrics
                     .Where(x => x.Provider == providerName)
                     .Where(x => x.Name == MetricName.DotNet.Project.CODE_DEPENDENCY_GRAPH && x is IResourceMetricValue<DirectedGraph>)
                     .Cast<IResourceMetricValue<DirectedGraph>>()
                     .Select(m => m.Value)
                     .Where(graph => graph != null)
                     .Cast<DirectedGraph>() // Cast to non-nullable type after null check
                     .ToList();

            var mergedGraph = DirectedGraph.MergeGraph(projectDependencies);
            return mergedGraph;
        }

        /// <summary>
        /// Gets the currently loaded graph (if any)
        /// </summary>
        /// <returns>The loaded DirectedGraph or null if not loaded</returns>
        public static DirectedGraph GetLoadedGraph()
        {
            return _source;
        }

        private static List<IMetric> GetAllMetrics(string metricFolder)
        {
            //metricFolder = @"C:\Dev\SampleAppSageWorkspace1\Output";
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
                    if (metrics != null)
                    {
                        result.AddRange(metrics);
                    }
                }
            }
            return result;
        }
    }
}
