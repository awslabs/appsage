using AppSage.Core.Metric;
using AppSage.Web.Components.Filter;
using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Const;
using System.Text.Json;
using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;

namespace AppSage.Web.Pages.Reports.DotNet.DependencyAnalysis
{
    public class IndexModel : MetricFilterPageModel
    {
        public IndexModel(IMetricReader metricReader) : base(metricReader) { }
        public IndexViewModel Dashboard { get; set; } = new IndexViewModel();   

        public override List<IMetric> GetMyMetrics()
        {
            //in this report we are using only those stats reported by AppSage.Providers.DotNet.DotNetDependencyAnalysisProvider
            string providerName = "AppSage.Providers.DotNet.DependencyAnalysis.DotNetDependencyAnalysisProvider";

            var allMetrics = GetAllMetrics();
            var result = allMetrics.Where(x => x.Provider == providerName).ToList();
            return result;
        }


        protected override void LoadData()
        {
            var metrics = GetFilteredMetrics();
            
            // Load project-level dependency graphs
            var projectDependencies = metrics
                .Where(x => x.Name == MetricName.DotNet.Project.CODE_DEPENDENCY_GRAPH && x is IResourceMetricValue<DirectedGraph>)
                .Cast<IResourceMetricValue<DirectedGraph>>()
                .ToList();

            // Load merged dependency graphs
            var mergedDependencies = metrics
                .Where(x => x.Name == MetricName.DotNet.MERGED_CODE_DEPENDENCY_GRAPH && x is IMetricValue<DirectedGraph>)
                .Cast<IMetricValue<DirectedGraph>>()
                .ToList();

            // Convert to graph data for visualization
            Dashboard.ProjectDependencyGraphs = ConvertToGraphData(projectDependencies);
            Dashboard.MergedDependencyGraphs = ConvertMergedToGraphData(mergedDependencies);

            // Collect all unique relation types for filtering
            var allRelationTypes = new HashSet<string>();
            foreach (var graph in Dashboard.ProjectDependencyGraphs.Concat(Dashboard.MergedDependencyGraphs))
            {
                foreach (var edge in graph.Edges)
                {
                    allRelationTypes.Add(edge.RelationType);
                }
            }
            Dashboard.AllRelationTypes = allRelationTypes.OrderBy(x => x).ToList();
        }

        private List<GraphData> ConvertToGraphData(List<IResourceMetricValue<DirectedGraph>> projectDependencies)
        {
            var graphDataList = new List<GraphData>();
            
            foreach (var projectDep in projectDependencies)
            {
                var graph = projectDep.Value;
                if (graph == null) continue;

                var graphData = new GraphData
                {
                    Title = $"Project: {Path.GetFileNameWithoutExtension(projectDep.Resource)}"
                };

                // Convert nodes
                var nodeCategories = new Dictionary<string, int>();
                var categoryIndex = 0;

                foreach (var node in graph.Nodes)
                {
                    // Determine category based on node type (namespace, class, etc.)
                    string category = DetermineNodeCategory(node.Id);
                    if (!nodeCategories.ContainsKey(category))
                    {
                        nodeCategories[category] = categoryIndex++;
                        graphData.Categories.Add(category);
                    }

                    graphData.Nodes.Add(new GraphNode
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Category = nodeCategories[category],
                        SymbolSize = Math.Max(20, Math.Min(60, node.Id.Length / 2)) // Size based on name length
                    });
                }

                // Convert edges
                foreach (var edge in graph.Edges)
                {
                    // The edge.ReferenceName contains the relationship type
                    string relationshipType = ExtractRelationshipType(edge.Name);
                    
                    graphData.Edges.Add(new GraphEdge
                    {
                        Source = edge.Source.Id,
                        Target = edge.Target.Id,
                        Label = edge.Name,
                        RelationType = relationshipType
                    });
                }

                graphDataList.Add(graphData);
            }

            return graphDataList;
        }

        private List<GraphData> ConvertMergedToGraphData(List<IMetricValue<DirectedGraph>> mergedDependencies)
        {
            var graphDataList = new List<GraphData>();
            
            foreach (var mergedDep in mergedDependencies)
            {
                var graph = mergedDep.Value;
                if (graph == null) continue;

                var graphData = new GraphData
                {
                    Title = "Merged Dependencies"
                };

                // Convert nodes
                var nodeCategories = new Dictionary<string, int>();
                var categoryIndex = 0;

                foreach (var node in graph.Nodes)
                {
                    string category = DetermineNodeCategory(node.Id);
                    if (!nodeCategories.ContainsKey(category))
                    {
                        nodeCategories[category] = categoryIndex++;
                        graphData.Categories.Add(category);
                    }

                    graphData.Nodes.Add(new GraphNode
                    {
                        Id = node.Id,
                        Name = node.Name,
                        Category = nodeCategories[category],
                        SymbolSize = Math.Max(20, Math.Min(60, node.Id.Length / 2))
                    });
                }

                // Convert edges
                foreach (var edge in graph.Edges)
                {
                    // The edge.ReferenceName contains the relationship type
                    string relationshipType = ExtractRelationshipType(edge.Name);
                    
                    graphData.Edges.Add(new GraphEdge
                    {
                        Source = edge.Source.Id,
                        Target = edge.Target.Id,
                        Label = edge.Name,
                        RelationType = relationshipType
                    });
                }

                graphDataList.Add(graphData);
            }

            return graphDataList;
        }

        private string ExtractRelationshipType(string edgeName)
        {
            // Edge names from provider could be in different formats:
            // 1. Simple: "Calls", "Uses", "Inherits", etc.
            // 2. Complex: "ClassA->ClassB::MethodName" where the relation type is embedded
            
            // First check if the edge name itself is a known relationship type
            var relationshipTypes = new[] { "Calls", "Uses", "Inherits", "Implements", "Accesses", "Creates", "Declares", "ProjectReference", "AssemblyReference" };
            
            // Direct match
            foreach (var type in relationshipTypes)
            {
                if (edgeName.Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }
            
            // Check if the relationship type is contained in the edge name
            foreach (var type in relationshipTypes)
            {
                if (edgeName.Contains(type, StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }
            
            // Try to extract from compound edge names like "ClassA->ClassB::MethodName"
            // The format might include the relationship type after "::"
            if (edgeName.Contains("::"))
            {
                var parts = edgeName.Split("::");
                if (parts.Length > 1)
                {
                    var lastPart = parts[parts.Length - 1];
                    // Check if the last part matches a known relationship type
                    foreach (var type in relationshipTypes)
                    {
                        if (lastPart.Equals(type, StringComparison.OrdinalIgnoreCase))
                        {
                            return type;
                        }
                    }
                }
            }
            
            // Default fallback - return the edge name as-is
            return edgeName;
        }

        private string DetermineNodeCategory(string nodeId)
        {
            if (nodeId.Contains('.'))
            {
                if (nodeId.Split('.').Length > 3)
                    return "Class";
                else
                    return "Namespace";
            }
            return "Assembly";
        }

        public string GetGraphDataJson(GraphData graphData)
        {
            return JsonSerializer.Serialize(graphData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public string GetAllRelationTypesJson()
        {
            return JsonSerializer.Serialize(Dashboard.AllRelationTypes);
        }
    }
}
