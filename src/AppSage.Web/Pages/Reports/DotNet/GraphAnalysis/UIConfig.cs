using AppSage.Core.ComplexType.Graph;

namespace AppSage.Web.Pages.Reports.DotNet.GraphAnalysis
{
    /// <summary>
    /// Contains UI configuration for displaying nodes , edges , legend and filters in the graph analysis.
    /// </summary>
    public class UIConfig
    {
        #region Node Type Configuration Methods
        internal static int GetNodeSize(INode node)
        {
            // Size based on lines of code if available
            //if (node.Attributes.ContainsKey(AppSage.Providers.DotNet.ConstString.Dependency.Attributes.CodeSegment.LinesOfCode) &&
            //    int.TryParse(node.Attributes[AppSage.Providers.DotNet.ConstString.Dependency.Attributes.CodeSegment.LinesOfCode], out int loc))
            //{
            //    return Math.Max(20, Math.Min(80, 20 + (loc / 50))); // Scale size based on lines of code
            //}

            return 30; // Default size
        }
        internal static string GetNodeColor(INode node)
        {
            return GetNodeColor(node.Type);
        }
        internal static string GetNodeColor(string nodeType)
        {
            return nodeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.PROJECT => "#1976d2",      // Blue
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.CLASS => "#388e3c",        // Green
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.INTERFACE => "#9c27b0",    // Purple
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ENUM => "#ff9800",         // Orange
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.STRUCT => "#607d8b",       // Blue Grey
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.DELEGATE => "#795548",     // Brown
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.METHOD => "#f57c00",       // Deep Orange
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ASSEMBLY => "#7b1fa2",     // Purple
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.NAMESPACE => "#00796b",     // Teal
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.SOLUTION => "#d32f2f",     // Red
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.MISCELLANEOUS => "#9e9e9e", // Grey
                _ => "#666666" // Default gray
            };
        }
        internal static string GetNodeShape(INode node)
        {
            return GetNodeShape(node.Type);
        }
        internal static string GetNodeShape(string nodeType)
        {
            return nodeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.PROJECT => "rectangle",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.CLASS => "ellipse",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.INTERFACE => "triangle",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ENUM => "pentagon",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.STRUCT => "square",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.DELEGATE => "star",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.METHOD => "triangle",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ASSEMBLY => "hexagon",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.NAMESPACE => "roundrectangle",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.SOLUTION => "diamond",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.MISCELLANEOUS => "octagon",
                _ => "ellipse"
            };
        }
        internal static string GetNodeTypeDescription(string nodeType)
        {
            return nodeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.PROJECT => "A .NET project containing code files and references",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.CLASS => "A class definition that encapsulates data and behavior",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.INTERFACE => "An interface that defines a contract for implementing classes",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ENUM => "An enumeration that defines a set of named constants",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.STRUCT => "A value type that encapsulates data and related functionality",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.DELEGATE => "A delegate that represents references to methods",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.METHOD => "A method that performs a specific operation",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ASSEMBLY => "A compiled library or executable containing IL code",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.NAMESPACE => "A logical grouping of related types and sub-namespaces",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.SOLUTION => "A Visual Studio solution containing multiple projects",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.MISCELLANEOUS => "Other code elements not fitting standard categories",
                "No Type" => "Element without a specific type classification",
                _ => $"Custom node type: {nodeType}"
            };
        }
        internal static string GetNodeTypeDisplayName(string nodeType)
        {
            return nodeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.PROJECT => "Project",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.CLASS => "Class",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.INTERFACE => "Interface",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ENUM => "Enum",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.STRUCT => "Struct",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.DELEGATE => "Delegate",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.METHOD => "Method",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.ASSEMBLY => "Assembly",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.NAMESPACE => "Namespace",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.SOLUTION => "Solution",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.NodeType.MISCELLANEOUS => "Miscellaneous",
                "No Type" => "Unknown Type",
                _ => nodeType
            };
        }


        #endregion

        #region Edge Type Configuration Methods

        internal static int GetEdgeWidth(IEdge edge)
        {
            return GetEdgeWidth(edge.Type);
        }
        internal static int GetEdgeWidth(string edgeType)
        {
            return edgeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INHERIT => 4,     // Thickest - strong relationship
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.IMPLEMENT => 3,  // Thick - interface contract
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.HAS => 3,        // Thick - composition
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.CREATE => 2,     // Medium - object creation
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.USE => 2,        // Medium - usage
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INVOKE => 2,     // Medium - method calls
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.ACCESS => 1,     // Thin - property access
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.DECLARE => 1,    // Thin - declarations
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.REFER => 1,      // Thinnest - loose coupling
                _ => 2 // Default
            };
        }
        internal static string GetEdgeColor(IEdge edge)
        {
            return GetEdgeColor(edge.Type);
        }
        internal static string GetEdgeColor(string edgeType)
        {
            return edgeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INHERIT => "#d32f2f",     // Red - strong coupling
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.IMPLEMENT => "#1976d2",  // Blue - interface contract
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.USE => "#388e3c",        // Green - usage
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INVOKE => "#f57c00",     // Orange - method calls
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.ACCESS => "#7b1fa2",     // Purple - property/field access
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.CREATE => "#00796b",     // Teal - object creation
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.DECLARE => "#ff5722",    // Deep Orange - declarations
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.REFER => "#757575",      // Gray - loose coupling
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.HAS => "#4caf50",       // Light Green - composition
                _ => "#cccccc" // Default light gray
            };
        }
        internal static string GetEdgeLineStyle(IEdge edge)
        {
            return GetEdgeLineStyle(edge.Type);
        }
        internal static string GetEdgeLineStyle(string edgeType)
        {
            return edgeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.REFER => "dashed",    // Dashed for loose coupling
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.DECLARE => "dotted", // Dotted for declarations
                _ => "solid" // Solid for most relationships
            };
        }
        internal static string GetEdgeArrowShape(IEdge edge)
        {
            return GetEdgeArrowShape(edge.Type);
        }
        internal static string GetEdgeArrowShape(string edgeType)
        {
            return edgeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INHERIT => "triangle-backcurve",  // Special arrow for inheritance
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.IMPLEMENT => "triangle-cross",   // Cross arrow for interface implementation
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.CREATE => "diamond",             // Diamond for object creation
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.HAS => "vee",                    // Vee for composition
                _ => "triangle" // Standard triangle arrow for most relationships
            };
        }
        internal static string GetEdgeTypeDescription(string edgeType)
        {
            return edgeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INHERIT => "Class inheritance relationship - 'is-a' relationship",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.IMPLEMENT => "Interface implementation - class implements interface contract",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.USE => "General usage dependency - type is used as field, property, or method signature",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INVOKE => "Method invocation - one type calls methods on another type",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.ACCESS => "Property or field access - direct access to type members",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.CREATE => "Object instantiation - one type creates instances of another",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.DECLARE => "Variable declaration - type is declared as a variable",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.REFER => "General reference - loose coupling or project/assembly reference",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.HAS => "Composition relationship - 'has-a' relationship",
                "No Type" => "Relationship without a specific type classification",
                _ => $"Custom relationship type: {edgeType}"
            };
        }

        internal static string GetEdgeTypeDisplayName(string edgeType)
        {
            return edgeType switch
            {
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INHERIT => "Inherit",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.IMPLEMENT => "Implement",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.USE => "Use",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.INVOKE => "Invoke",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.ACCESS => "Access",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.CREATE => "Create",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.DECLARE => "Declare",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.REFER => "Refer",
                var type when type == AppSage.Providers.DotNet.ConstString.Dependency.DependencyType.HAS => "Has",
                "No Type" => "Unknown Relationship",
                _ => edgeType
            };
        }

        #endregion

        internal static string GetDisplayName(string attributeKey)
        {
            return attributeKey switch
            {
                
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.RepositoryName => "Repository Name",
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.ProjectClassCount => "Project Class Count",
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.ProjectMethodCount => "Project Method Count",
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.ProjectDataNameTypeCount => "Project Data Class Count",
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.ProjectLinesOfCode => "Project Lines of Code",


                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.NameTypeMethodCount => "Class Method Count",
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.NameTypeLinesOfCode => "Class Lines of Code",
                
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.MethodLinesOfCode => "Method Lines of Code",
 
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.AssemblyName => "Assembly Name",
                var key when key == AppSage.Providers.DotNet.ConstString.Dependency.Attributes.AssemblyVersion => "Assembly Version",
     
                _ => attributeKey
            };
        }
    }
}
