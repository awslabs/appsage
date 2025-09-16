namespace AppSage.Web.Pages.Reports.DotNet.DependencyAnalysis
{
    public class IndexViewModel
    {
        public List<GraphData> ProjectDependencyGraphs { get; set; } = new();
        public List<GraphData> MergedDependencyGraphs { get; set; } = new();
        public List<string> AllRelationTypes { get; set; } = new();
    }

    public class GraphData
    {
        public List<GraphNode> Nodes { get; set; } = new();
        public List<GraphEdge> Edges { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public string Title { get; set; } = string.Empty;
    }

    public class GraphNode
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Category { get; set; }
        public int SymbolSize { get; set; } = 30;
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class GraphEdge
    {
        public string Source { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string RelationType { get; set; } = string.Empty;
    }
}
