using AppSage.Core.ComplexType.Graph;
using AppSage.Core.Configuration;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Web.Components.Filter;

namespace AppSage.Web.Pages.Reports.DotNet.GraphAnalysis
{
    public class IndexModel : MetricFilterPageModel
    {
        public IndexModel(IAppSageLogger logger, IAppSageConfiguration config, IAppSageWorkspace workspace) : base(logger, config, workspace) { }
        public IndexViewModel Dashboard { get; set; } = new IndexViewModel();
        private static DirectedGraph _lazyGraph;
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

            DirectedGraph mergedGraph = null;
            if (_lazyGraph == null)
            {
                lock (this)
                {
                    if (_lazyGraph == null)
                    {
                        var metrics = GetFilteredMetrics();

                        // Load project-level dependency graphs
                        var projectDependencies = metrics
                            .Where(x => x.Name == MetricName.DotNet.Project.CODE_DEPENDENCY_GRAPH && x is IResourceMetricValue<DirectedGraph>)
                            .Cast<IResourceMetricValue<DirectedGraph>>()
                            .Select(m => m.Value)
                            .Where(graph => graph != null)
                            .Cast<DirectedGraph>() // Cast to non-nullable type after null check
                            .ToList();

                        var g = DirectedGraph.MergeGraph(projectDependencies);
                        _lazyGraph = MyQuery(g);
                        
                    }
                }
            }
    
            mergedGraph= _lazyGraph;


            PopulateFilters(mergedGraph);
            PopulateNodeLegend(mergedGraph);
            PopulateEdgeLegend(mergedGraph);
            PopulateGraph(mergedGraph);
        }




        private DirectedGraph MyQuery(DirectedGraph sourceGraph) {

            return sourceGraph;
            var result = new DirectedGraph();

            sourceGraph.Nodes.Where(n=>n.Type=="Project").ToList().ForEach(node => result.AddOrUpdateNode(node));
            sourceGraph.Edges.Where(e => e.Source.Type == "Project" && e.Target.Type == "Project").ToList().ForEach(edge => result.AddOrUpdateEdge(edge));

            return result;

        }

        /// <summary>
        /// Populates the filters for nodes and edges based on the provided graph.
        /// </summary>
        private void PopulateFilters(DirectedGraph graph)
        {
            // Node filters - only extract node types
            var nodeTypes = new HashSet<string>();

            foreach (var node in graph.Nodes)
            {
                // Extract node types - using Type property directly
                nodeTypes.Add(node.Type);
            }

            // Add node type filter
            Dashboard.NodeFilters.Add("Node Type", new MultiSelectFilter
            {
                AvailableValues = nodeTypes,
                SelectedValues = new HashSet<string>(nodeTypes), // All selected by default
                AttributeKey = "Type" // Using the Type property directly
            });

            // Edge filters - only extract edge types
            var edgeTypes = new HashSet<string>();

            foreach (var edge in graph.Edges)
            {
                // Extract edge types - using Type property directly
                edgeTypes.Add(edge.Type);
            }

            // Add edge type filter
            Dashboard.EdgeFilters.Add("Edge Type", new MultiSelectFilter
            {
                AvailableValues = edgeTypes,
                SelectedValues = new HashSet<string>(edgeTypes), // All selected by default
                AttributeKey = "Type" // Using the Type property directly
            });

            // Initialize default edge distances for relationships
            InitializeEdgeDistances();

            // Initialize grouping configurations
            InitializeGroupingConfigs(nodeTypes);
        }

        /// <summary>
        /// Populates the graph data structure for visualization with nodes and edges from the directed graph.
        /// </summary>
        private void PopulateGraph(DirectedGraph directedGraph)
        {
            var graphData = new GraphData
            {
                Title = "Code Dependency Analysis",
                Metadata = new Dictionary<string, object>
                {
                    ["nodeCount"] = directedGraph.Nodes.Count,
                    ["edgeCount"] = directedGraph.Edges.Count
                }
            };

            // Convert nodes
            foreach (var node in directedGraph.Nodes)
            {
                var graphNode = new GraphNode
                {
                    Data = new NodeData
                    {
                        Id = node.Id,
                        Label = node.Name,
                        Type = node.Type,
                        Attributes = new Dictionary<string, string>(node.Attributes),
                        Color = UIConfig.GetNodeColor(node),
                        Shape = UIConfig.GetNodeShape(node),
                        Size = UIConfig.GetNodeSize(node)
                    }
                };

                graphData.Nodes.Add(graphNode);
            }

            // Convert edges
            foreach (var edge in directedGraph.Edges)
            {
                var edgeColor = UIConfig.GetEdgeColor(edge);
                var graphEdge = new GraphEdge
                {
                    Data = new EdgeData
                    {
                        Id = edge.Id,
                        Source = edge.Source.Id,
                        Target = edge.Target.Id,
                        Label = edge.Name,
                        Type = edge.Type,
                        Attributes = new Dictionary<string, string>(edge.Attributes),
                        Color = edgeColor,
                        Width = UIConfig.GetEdgeWidth(edge),
                        LineStyle = UIConfig.GetEdgeLineStyle(edge),
                        TargetArrowShape = UIConfig.GetEdgeArrowShape(edge),
                        TargetArrowColor = edgeColor  // Set arrow color same as edge color
                    }
                };

                graphData.Edges.Add(graphEdge);
            }

            Dashboard.MainGraph = graphData;
        }

        /// <summary>
        /// Populates the node legend for visulization with unique node types and their visual properties.
        /// </summary>
        private void PopulateNodeLegend(DirectedGraph graph)
        {
            var nodeTypes = new HashSet<string>();

            // Extract unique node types from the graph
            foreach (var node in graph.Nodes)
            {
                nodeTypes.Add(node.Type);
            }

            // Create legend items for each node type
            foreach (var nodeType in nodeTypes.OrderBy(x => x))
            {
                var legendItem = new NodeLegendItem
                {
                    Type = nodeType,
                    DisplayName = UIConfig.GetNodeTypeDisplayName(nodeType),
                    Color = UIConfig.GetNodeColor(nodeType),
                    Shape = UIConfig.GetNodeShape(nodeType),
                    Description = UIConfig.GetNodeTypeDescription(nodeType),
                    IsVisible = true
                };

                Dashboard.NodeLegend.Add(legendItem);
            }
        }

        /// <summary>
        /// Populates the edge legend for visualization with unique edge types and their visual properties.
        /// </summary>
        private void PopulateEdgeLegend(DirectedGraph graph)
        {
            var edgeTypes = new HashSet<string>();

            // Extract unique edge types from the graph
            foreach (var edge in graph.Edges)
            {
                edgeTypes.Add(edge.Type);
            }

            // Create legend items for each edge type
            foreach (var edgeType in edgeTypes.OrderBy(x => x))
            {
                var legendItem = new EdgeLegendItem
                {
                    Type = edgeType,
                    DisplayName = UIConfig.GetEdgeTypeDisplayName(edgeType),
                    Color = UIConfig.GetEdgeColor(edgeType),
                    Width = UIConfig.GetEdgeWidth(edgeType),
                    LineStyle = UIConfig.GetEdgeLineStyle(edgeType),
                    ArrowShape = UIConfig.GetEdgeArrowShape(edgeType),
                    Description = UIConfig.GetEdgeTypeDescription(edgeType),
                    IsVisible = true
                };

                Dashboard.EdgeLegend.Add(legendItem);
            }
        }


        private void InitializeEdgeDistances()
        {
            Dashboard.EdgeDistances = new Dictionary<string, int>
            {
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INHERIT] = 20,    // Close - strong coupling
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.IMPLEMENT] = 25,  // Close - interface implementation
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.USE] = 40,        // Medium - usage dependency
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INVOKE] = 60,     // Medium-far - method calls
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.ACCESS] = 50,     // Medium - property/field access
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.CREATE] = 45,     // Medium - object creation
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.DECLARE] = 35,    // Close-medium - variable declaration
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.REFER] = 80,      // Far - loose coupling
                [AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.HAS] = 30         // Close - composition
            };
        }

        private void InitializeGroupingConfigs(HashSet<string> nodeTypes)
        {
            Dashboard.GroupingConfigs = new Dictionary<string, GroupingConfig>
            {
                ["Project"] = new GroupingConfig
                {
                    Name = "Group by Project",
                    GroupByAttribute = "Type", // Changed to use Type property directly
                    GroupByValue = AppSage.Providers.DotNet.ConstString.Dependency.NodeType.PROJECT,
                    Enabled = false,
                    Color = "#e3f2fd",
                    BorderColor = "#1976d2"
                },
                ["Assembly"] = new GroupingConfig
                {
                    Name = "Group by Assembly",
                    GroupByAttribute = "Type", // Changed to use Type property directly
                    GroupByValue = AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ASSEMBLY,
                    Enabled = false,
                    Color = "#f3e5f5",
                    BorderColor = "#7b1fa2"
                },
                ["Namespace"] = new GroupingConfig
                {
                    Name = "Group by Namespace",
                    GroupByAttribute = "Type", // Changed to use Type property directly
                    GroupByValue = AppSage.Providers.DotNet.ConstString.Dependency.NodeType.NAMESPACE,
                    Enabled = false,
                    Color = "#e8f5e8",
                    BorderColor = "#388e3c"
                }
            };
        }






    }
}
