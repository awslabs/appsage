using System.Collections.Generic;
namespace AppSage.Core.ComplexType.Graph
{
    /// <summary>
    /// Represents a basic element in a graph (node or edge).
    /// </summary>
    public interface IGraphElement
    {
        /// <summary>
        /// Unique identifier for the graph element. Must be unique within a graph.
        /// Examples:
        ///   Node: 
        ///    - for project, this can be repository relative path of the project file, e.g. "MyRepository/src/MyProject/MyProject.csproj"
        ///    - for class, this can be fully qualified name, e.g. "MyNamespace.MyClass"
        ///    - for an interface, this can be fully qualified name, e.g. "MyNamespace.IMyInterface"
        ///    - for method, this can be fully qualified name of the class followed by : followed by Method name, e.g. "MyNamespce.MyClass:MyMethod
        ///    - for a person, this can be email address, e.g. "joe.doe@example.com"
        ///    - for complex systems, this can be a combination of type and name, e.g. "System:MySystem", "Subsystem:MySubsystem"
        ///   Edge: 
        ///   - It's recommended to use the static class AppSage.Core.ComplexType.Graph.GraphUtility to generate edge IDs.
        ///     - Example: GraphUtility.GetEdgeId(sourceNode, targetNode, edgeType)
        ///   - You can also manually create edge IDs using the format: "{sourceId}>[{edgeType}]>{targetId}"
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Human-readable, short name for the element. Not required to be unique. This is used for display purposes.
        /// Examples:
        ///   Node: "User Service", "OrderController", "MyCompany.WebAPI" or Name of the class or full qualified name of the class
        ///   Edge: "Depends on", "References", "Calls"
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Type/classification of the element. Used to distinguish element kinds.
        /// Examples:
        ///   For node this should be noun:  E.g. "Class", "Project", "Repository", "Person", "Interface", "Module", "Service", "Database"
        ///   For Edge this should be a verb: E.g.  "calls", "inherits", "depends on", "resides", "owns"
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Key-value attributes for the element. Used for custom metadata or hold additional information or payload.
        /// Example: 
        /// For Node: element.Attributes["color"] = "blue", element.Attributes["LinesOfCode"] = "1543", element.Attributes["CreatedDate"] = "2023-10-05"
        /// For Edge: element.Attributes["Weight"] = "5", element.Attributes["CreatedBy"] = "automation-script"
        /// </summary>
        IReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Add or update a single attribute.
        /// Input: key (string), value (string)
        /// Example: element.AddOrUpdateAttribute("color", "red")
        /// </summary>
        void AddOrUpdateAttribute(string key, string value);

        /// <summary>
        /// Add or update multiple attributes.
        /// Input: attributes (dictionary)
        /// Example: element.AddOrUpdateAttribute(new Dictionary&lt;string,string&gt; { {"size", "large"} })
        /// </summary>
        void AddOrUpdateAttribute(IReadOnlyDictionary<string, string> attributes);
    }
}
