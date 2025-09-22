using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Web.Components.Filter;
using Microsoft.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AppSage.Web.Pages.Reports.DotNet.PurposeAnalysis
{
    public class TreeNode
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public bool IsLeaf => !Children.Any();
        public List<TreeNode> Children { get; set; } = new();
        
        [JsonIgnore]
        public TreeNode? Parent { get; set; }
        public NodeType Type { get; set; }
    }

    public enum NodeType
    {
        Repository,
        Folder,
        File
    }

    public class IndexModel : MetricFilterPageModel
    {
        public IndexModel(IMetricReader metricReader) : base(metricReader) { }
        public TreeNode RootNode { get; set; } = new();
        public string TreeRootPrefix { get; set; } = "\\Repositories\\";

        public override List<IMetric> GetMyMetrics()
        {
            //in this report we are using only those stats reported by AppSage.Providers.DotNet.DotNetAIAnalysisProvider
            string providerName = "AppSage.Providers.DotNet.DotNetAIAnalysisProvider";

            var allMetrics = GetAllMetrics();
            var result = allMetrics.Where(x => x.Provider == providerName && x is IResourceMetricValue<string>).ToList();
            return result;
        }

        protected override void LoadData()
        {
            var metrics = GetFilteredMetrics().Select(x => x as IResourceMetricValue<string>).Where(x => x != null).ToList();

            // Build tree structure from metrics
            RootNode = BuildTreeFromMetrics(metrics!);
            

        }

        private int CountNodesWithSummaries(TreeNode node)
        {
            int count = string.IsNullOrEmpty(node.Summary) ? 0 : 1;

            foreach (var child in node.Children)
            {
                count += CountNodesWithSummaries(child);
            }
            return count;
        }

        private TreeNode BuildTreeFromMetrics(List<IResourceMetricValue<string>> metrics)
        {
            var root = new TreeNode
            {
                Id = "root",
                Name = "Root",
                Path = "",
                Type = NodeType.Repository
            };

            var nodeMap = new Dictionary<string, TreeNode>();
            nodeMap[""] = root;


            foreach (var metric in metrics)
            {
                if (string.IsNullOrEmpty(metric.Resource)) 
                {
                    continue;
                }

                // Find the tree root prefix in the resource path
                var resourcePath = metric.Resource.Replace('/', '\\');
                var rootIndex = resourcePath.IndexOf(TreeRootPrefix, StringComparison.OrdinalIgnoreCase);
                
                System.Diagnostics.Debug.WriteLine($"Looking for prefix '{TreeRootPrefix}' in '{resourcePath}', found at index: {rootIndex}");
                
                if (rootIndex == -1) 
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping - TreeRootPrefix not found in resource path");
                    continue;
                }

                // Extract the path starting from the root prefix
                var relativePath = resourcePath.Substring(rootIndex + TreeRootPrefix.Length);
                var pathSegments = relativePath.Split('\\', StringSplitOptions.RemoveEmptyEntries);

                System.Diagnostics.Debug.WriteLine($"Relative path: '{relativePath}', segments: [{string.Join(", ", pathSegments)}]");

                if (!pathSegments.Any()) 
                {
                    System.Diagnostics.Debug.WriteLine("Skipping - no path segments");
                    continue;
                }

                // Build the tree path
                var currentPath = "";
                TreeNode currentNode = root;

                for (int i = 0; i < pathSegments.Length; i++)
                {
                    var segment = pathSegments[i];
                    var segmentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}\\{segment}";
                    
                    if (!nodeMap.ContainsKey(segmentPath))
                    {
                        var isLastSegment = i == pathSegments.Length - 1;
                        var nodeType = i == 0 ? NodeType.Repository : 
                                      isLastSegment && (segment.Contains('.') || segment.EndsWith(".cs") || segment.EndsWith(".vb")) ? NodeType.File : 
                                      NodeType.Folder;

                        var newNode = new TreeNode
                        {
                            Id = segmentPath.Replace('\\', '_').Replace('.', '_'),
                            Name = segment,
                            Path = segmentPath,
                            Parent = currentNode,
                            Type = nodeType
                        };

                        System.Diagnostics.Debug.WriteLine($"Created new node: '{newNode.Name}' (Type: {nodeType}, Path: '{segmentPath}')");

                        currentNode.Children.Add(newNode);
                        nodeMap[segmentPath] = newNode;
                    }

                    currentNode = nodeMap[segmentPath];
                    currentPath = segmentPath;
                }

                // Add summary to the final node (file) - this happens after the path is built
                if (!string.IsNullOrEmpty(metric.Value) && nodeMap.ContainsKey(currentPath))
                {
                    var targetNode = nodeMap[currentPath];
                    System.Diagnostics.Debug.WriteLine($"Adding summary to node: {targetNode.Name} (Path: {targetNode.Path})");
                    if (string.IsNullOrEmpty(targetNode.Summary))
                    {
                        targetNode.Summary = metric.Value;
                        System.Diagnostics.Debug.WriteLine($"Added summary to {targetNode.Name}, length: {metric.Value.Length}");
                    }
                    else
                    {
                        // If there's already a summary, append this one
                        targetNode.Summary += "\n\n---\n\n" + metric.Value;
                        System.Diagnostics.Debug.WriteLine($"Appended summary to {targetNode.Name}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Skipping summary - Value null/empty: {string.IsNullOrEmpty(metric.Value)}, Node exists: {nodeMap.ContainsKey(currentPath)}, CurrentPath: '{currentPath}'");
                }
            }

            // Sort children alphabetically within each node
            SortTreeChildren(root);

            System.Diagnostics.Debug.WriteLine($"Finished building tree. Total nodes in map: {nodeMap.Count}");

            return root;
        }

        private void SortTreeChildren(TreeNode node)
        {
            // Sort: Repositories first, then folders, then files - each group alphabetically
            node.Children = node.Children.OrderBy(x => x.Type)
                                        .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                                        .ToList();

            foreach (var child in node.Children)
            {
                SortTreeChildren(child);
            }
        }

        public string GetTreeNodeIcon(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.Repository => "fa-solid fa-code-branch",
                NodeType.Folder => "fa-solid fa-folder",
                NodeType.File => "fa-solid fa-file-code",
                _ => "fa-solid fa-file"
            };
        }

    }
}
