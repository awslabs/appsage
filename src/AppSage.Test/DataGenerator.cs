using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Const;
using AppSage.Core.Metric;
using Newtonsoft.Json;

namespace AppSage.Test
{
    public class DataGenerator
    {
        [Test]
        public void GenerateData()
        {
            string metricFolder = @"C:\Dev\SampleAppSageWorkspace\Output\LastRun";
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


            var projectDependencies = result
            .Where(x => x.Name == MetricName.DotNet.Project.CODE_DEPENDENCY_GRAPH && x is IResourceMetricValue<DirectedGraph>)
            .Cast<IResourceMetricValue<DirectedGraph>>()
            .Select(m => m.Value)
            .Where(graph => graph != null)
            .Cast<DirectedGraph>() // Cast to non-nullable type after null check
            .ToList();

            var g = DirectedGraph.MergeGraph(projectDependencies);
            SaveGraph(g);

        }

        private static void SaveGraph(DirectedGraph graph)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented, // Pretty print
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, // Ignore nulls
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore // Prevent reference loop errors

            };

            string content = Newtonsoft.Json.JsonConvert.SerializeObject(graph, settings);

            string file = @"C:\temp\SampleGraph.appsage.graph";
            System.IO.File.WriteAllText(file, content);
        }
    }
}
