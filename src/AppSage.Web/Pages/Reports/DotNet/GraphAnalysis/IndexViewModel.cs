namespace AppSage.Web.Pages.Reports.DotNet.GraphAnalysis
{
    public class IndexViewModel
    {
        public GraphData MainGraph { get; set; } = new();

        /// <summary>
        /// Set of filters to filter nodes in the graph.
        /// key is the filter name. For each filter name UI component will be rendered to allow user to select values.
        /// </summary>
        public Dictionary<string, FilterPossibleValue> NodeFilters { get; set; } = new();
        public Dictionary<string, FilterPossibleValue> EdgeFilters { get; set; } = new();

        /// <summary>
        /// Grouping configurations for visual grouping of nodes
        /// </summary>
        public Dictionary<string, GroupingConfig> GroupingConfigs { get; set; } = new();

        /// <summary>
        /// Edge relationship distances for layout customization
        /// </summary>
        public Dictionary<string, int> EdgeDistances { get; set; } = new();

        /// <summary>
        /// Legend for node types with their visual representation
        /// </summary>
        public List<NodeLegendItem> NodeLegend { get; set; } = new();

        /// <summary>
        /// Legend for edge types with their visual representation
        /// </summary>
        public List<EdgeLegendItem> EdgeLegend { get; set; } = new();
    }

    /// <summary>
    /// Legend item for node types
    /// </summary>
    public class NodeLegendItem
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Shape { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
    }

    /// <summary>
    /// Legend item for edge types
    /// </summary>
    public class EdgeLegendItem
    {
        public string Type { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Width { get; set; } = 2;
        public string LineStyle { get; set; } = "solid";
        public string ArrowShape { get; set; } = "triangle";
        public string Description { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
    }

    public abstract class FilterPossibleValue
    {
        // This property is intentionally left empty to allow for polymorphism.
        // Concrete implementations will define the type of Value they hold.
    }

    /// <summary>
    /// Holds the actual values of the filter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FilterGenericPossibleValue<T> : FilterPossibleValue 
    {
        public T Value { get; set; } = default!;
    }

    /// <summary>
    /// Possible value for the filter is only one out of many. User will be given an option list to select from. Only one value can be selected. 
    /// </summary>
    public class SingleSelectSetFilterValue : FilterGenericPossibleValue<HashSet<string>>
    {
    }

    /// <summary>
    /// Possible value for the filter is a set of strings. User will be given a drop down with checker boxes to select multiple values.
    /// </summary>
    public class MultiSelectSetFilterValue : FilterGenericPossibleValue<HashSet<string>>
    {
    }

    /// <summary>
    /// Possible values for a filter is an integer number. User must be given a text box to enter the value.
    /// </summary>
    public class IntegerFilterValue : FilterGenericPossibleValue<int>
    {
    }

    /// <summary>
    /// Possible values for a filter is a range of integers. User must be able to give a range filter to select the range of values between Min and Max.
    /// </summary>
    public class IntegerRangeFilterValue : FilterPossibleValue
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }

    /// <summary>
    /// Graph data structure suitable for Cytoscape.js visualization
    /// </summary>
    public class GraphData
    {
        public List<GraphNode> Nodes { get; set; } = new();
        public List<GraphEdge> Edges { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Node data structure compatible with Cytoscape.js
    /// </summary>
    public class GraphNode
    {
        public NodeData Data { get; set; } = new();
        public NodePosition Position { get; set; } = new();
        public string Group { get; set; } = "nodes";
        public bool Selected { get; set; } = false;
        public bool Selectable { get; set; } = true;
        public bool Locked { get; set; } = false;
        public bool Grabbable { get; set; } = true;
        public Dictionary<string, object> Classes { get; set; } = new();
    }

    /// <summary>
    /// Node data for Cytoscape.js
    /// </summary>
    public class NodeData
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Parent { get; set; } = string.Empty; // For grouping
        public Dictionary<string, string> Attributes { get; set; } = new();
        public int Weight { get; set; } = 1;
        public string Color { get; set; } = "#666";
        public int Size { get; set; } = 30;
        public string Shape { get; set; } = "ellipse";
    }

    /// <summary>
    /// Node position for Cytoscape.js
    /// </summary>
    public class NodePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Edge data structure compatible with Cytoscape.js
    /// </summary>
    public class GraphEdge
    {
        public EdgeData Data { get; set; } = new();
        public string Group { get; set; } = "edges";
        public bool Selected { get; set; } = false;
        public bool Selectable { get; set; } = true;
        public Dictionary<string, object> Classes { get; set; } = new();
    }

    /// <summary>
    /// Edge data for Cytoscape.js
    /// </summary>
    public class EdgeData
    {
        public string Id { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new();
        public int Weight { get; set; } = 1;
        public string Color { get; set; } = "#ccc";
        public int Width { get; set; } = 2;
        public string LineStyle { get; set; } = "solid";
        public string TargetArrowShape { get; set; } = "triangle";
        public string TargetArrowColor { get; set; } = "#ccc";
    }

    /// <summary>
    /// Configuration for grouping nodes visually
    /// </summary>
    public class GroupingConfig
    {
        public string Name { get; set; } = string.Empty;
        public string GroupByAttribute { get; set; } = string.Empty;
        public string GroupByValue { get; set; } = string.Empty;
        public bool Enabled { get; set; } = false;
        public string Color { get; set; } = "#f0f0f0";
        public string BorderColor { get; set; } = "#999";
    }

    /// <summary>
    /// Range filter specifically for integer ranges with current selected values
    /// </summary>
    public class IntegerRangeFilter : FilterPossibleValue
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int CurrentMin { get; set; }
        public int CurrentMax { get; set; }
        public string AttributeKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Multi-select filter with current selected values
    /// </summary>
    public class MultiSelectFilter : FilterPossibleValue
    {
        public HashSet<string> AvailableValues { get; set; } = new();
        public HashSet<string> SelectedValues { get; set; } = new();
        public string AttributeKey { get; set; } = string.Empty;
    }
}
