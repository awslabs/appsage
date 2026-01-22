using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Query;
using AppSage.Core.Template;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Serialization;
using AppSage.Query;
using Newtonsoft.Json.Linq;
using System.Data;

namespace AppSage.Infrastructure.Query
{
    public class TemplateQueryManager : ITemplateQuery
    {
        static IDirectedGraph _graph = null;

        private static object _graphLock = new object();

        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        ITemplateManager _templateManager;
        IDynamicCompiler _dynamicCompiler;
        IMetricReader _metricReader;
        public TemplateQueryManager(IAppSageLogger logger, IAppSageWorkspace workspace, ITemplateManager templateManager, IMetricReader metricReader, IDynamicCompiler compiler)
        {
            _logger = logger;
            _workspace = workspace;
            _templateManager = templateManager;
            _dynamicCompiler = compiler;
            _metricReader = metricReader;
        }
        public void Run(string templateId)
        {
            var templates = _templateManager.GetTemplates(templateId);
            LoadGraphData();
            foreach (var template in templates)
            {
                _logger.LogInformation($"Running template: {template.TemplateId}");
                try
                {
                    var result = _dynamicCompiler.CompileAndExecute(template.Content, _graph);
                    Save(result.ExecutionResult, template.TemplateId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error running the template: {template.TemplateId}", ex);
                }
            }

        }

        private void Save(Object? result, string templateId)
        {
            if (result == null)
            {
                _logger.LogInformation("Nothing is saved as the result is null.");
                return;
            }

            string filePrefix = templateId.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
            string fileSuffix = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{Guid.NewGuid().ToString().Substring(0, 6)}";

            // Handle IEnumerable<DataTable> - convert to Excel file
            if (result is IEnumerable<DataTable> dataTables)
            {
                try
                {

                    var workbook = Utility.ConvertToExcelDoc(dataTables);
                    var fileName = $"{filePrefix}_Report_{fileSuffix}.xlsx";

                    // Use memory stream instead of temporary files
                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);

                    string filePath = Path.Combine(_workspace.TemplateBasedAnalysisOutputFolder, fileName);
                    File.WriteAllBytes(filePath, stream.ToArray());

                    _logger.LogInformation($"Excel file has been created at: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error converting DataTables to Excel", ex);
                }
            }
            else if (result is DirectedGraph graph)
            {
                // Handle DirectedGraph - convert to .appsagegraph file
                try
                {
                    var fileName = $"{filePrefix}_Graph_{fileSuffix}.appsagegraph";
                    string filePath = Path.Combine(_workspace.TemplateBasedAnalysisOutputFolder, fileName);
                    AppSageSerializer.SerializeToFile(filePath, graph);
                    _logger.LogInformation($"Graph file has been created at: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error saving the graph", ex);
                }

            }
            else if (result is JObject jObject)
            {
                // Handle JObject from Newtonsoft.Json - convert to JSON file
                try
                {
                    var jsonString = jObject.ToString(Newtonsoft.Json.Formatting.Indented);


                    var fileName = $"{filePrefix}_JObject_{fileSuffix}.json";


                    File.WriteAllText(Path.Combine(_workspace.TemplateBasedAnalysisOutputFolder, fileName), jsonString);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error converting JObject to json content", ex);
                }
            }
            else
            {

                var textFile = $"{filePrefix}_Text_{fileSuffix}.txt";
                var text = Convert.ToString(result) ?? string.Empty;
                File.WriteAllText(Path.Combine(_workspace.TemplateBasedAnalysisOutputFolder, textFile), text);
            }

        }



        private void LoadGraphData()
        {
            lock (_graphLock)
            {
                if (_graph == null)
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

                    _logger.LogInformation("Loading completed.");

                }
                else
                {
                    _logger.LogInformation("Graph data is already loaded.");
                }
            }
            _logger.LogInformation($"The graph has {_graph.Nodes.Count} nodes and {_graph.Edges.Count} edges.");
        }
    }
}
