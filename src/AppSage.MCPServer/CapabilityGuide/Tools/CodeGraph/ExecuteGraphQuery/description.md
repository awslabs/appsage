CodeGraph run custom analyis on the code graph data by executing the C# code given in the parameter ```codeToCompileAndRun```.
It will return Directed graph with the results of the analysis.
```codeToCompileAndRun``` will have a custom code you want to execute on the code graph data. 

```codeToCompileAndRun``` must have a .NET C# public class ```public class MyQuery``` with a  public static method with the following signature:
```public static DirectedGraph Execute(IDirectedGraph graph)```. 

```DirectedGraph``` implements the interface ```IDirectedGraph```.

It's critical that the method signature is exactly as specified above and that the class is called MyQuery.

Interface definitions are given below for reference:
```csharp
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

namespace AppSage.Core.ComplexType.Graph
{
    public interface INode:IGraphElement
    {
    }
}

namespace AppSage.Core.ComplexType.Graph
{
    /// <summary>
    /// Represents a directed edge in a graph, connecting a source node to a target node.
    /// Inherits common properties from IGraphElement (Id, Name, Type, Attributes).
    /// </summary>
    public interface IEdge : IGraphElement
    {
        /// <summary>
        /// The source node of the edge (where the edge starts).
        /// Example: In a "calls" edge, Source is the caller node.
        /// </summary>
        INode Source { get; set; }

        /// <summary>
        /// The target node of the edge (where the edge points to).
        /// Example: In a "calls" edge, Target is the callee node.
        /// </summary>
        INode Target { get; set; }
    }
}

namespace AppSage.Core.ComplexType.Graph
{
    public interface IDirectedGraph
    {
        /// <summary>
        /// List of all nodes in the graph.
        /// Example: var nodes = graph.Nodes;
        /// </summary>
        IReadOnlyList<INode> Nodes { get; }

        /// <summary>
        /// List of all edges in the graph.
        /// Example: var edges = graph.Edges;
        /// </summary>
        IReadOnlyList<IEdge> Edges { get; }

        /// <summary>
        /// Checks if the node exists.
        /// Input: node (INode)
        /// Output: true if found.
        /// Example: graph.ContainsNode(node)
        /// </summary>
        bool ContainsNode(INode node);

        /// <summary>
        /// Checks if the edge exists.
        /// Input: edge (IEdge)
        /// Output: true if found.
        /// Example: graph.ContainsEdge(edge)
        /// </summary>
        bool ContainsEdge(IEdge edge);

        /// <summary>
        /// Get node by id.
        /// Input: id (string)
        /// Output: INode or null.
        /// Example: graph.GetNode("n1")
        /// </summary>
        INode GetNode(string id);

        /// <summary>
        /// Get edge by id.
        /// Input: id (string)
        /// Output: IEdge or null.
        /// Example: graph.GetEdge("e1")
        /// </summary>
        IEdge GetEdge(string id);

        /// <summary>
        /// Add or update a node by id.
        /// Input: id (string)
        /// Output: INode.
        /// Example: graph.AddOrUpdateNode("n1")
        /// </summary>
        INode AddOrUpdateNode(string id);

        /// <summary>
        /// Add or update a node by id and name.
        /// Input: id, name (string)
        /// Output: INode.
        /// Example: graph.AddOrUpdateNode("n1", "Node1")
        /// </summary>
        INode AddOrUpdateNode(string id, string name);

        /// <summary>
        /// Add or update a node by id, name, type.
        /// Input: id, name, type (string)
        /// Output: INode.
        /// Example: graph.AddOrUpdateNode("n1", "Node1", "TypeA")
        /// </summary>
        INode AddOrUpdateNode(string id, string name, string type);

        /// <summary>
        /// Add or update a node with attributes.
        /// Input: id, name, type (string), attributes (dict)
        /// Output: INode.
        /// Example: graph.AddOrUpdateNode("n1", "Node1", "TypeA", attrs)
        /// </summary>
        INode AddOrUpdateNode(string id, string name, string type, IReadOnlyDictionary<string, string> attributes);

        /// <summary>
        /// Add or update a node object.
        /// Input: node (INode)
        /// Output: INode.
        /// Example: graph.AddOrUpdateNode(node)
        /// </summary>
        INode AddOrUpdateNode(INode node);

        /// <summary>
        /// Add or update an edge by id, source, target.
        /// Input: id (string), source, target (INode)
        /// Output: IEdge.
        /// Example: graph.AddOrUpdateEdge("e1", src, tgt)
        /// </summary>
        IEdge AddOrUpdateEdge(string id, INode source, INode target);

        /// <summary>
        /// Add or update an edge by source, target, type.
        /// Input: source, target (INode), type (string)
        /// Output: IEdge.
        /// Example: graph.AddOrUpdateEdge(src, tgt, "calls")
        /// </summary>
        IEdge AddOrUpdateEdge(INode source, INode target, string type);

        /// <summary>
        /// Add or update an edge by id, name, source, target.
        /// Input: id, name (string), source, target (INode)
        /// Output: IEdge.
        /// Example: graph.AddOrUpdateEdge("e1", "Edge1", src, tgt)
        /// </summary>
        IEdge AddOrUpdateEdge(string id, string name, INode source, INode target);

        /// <summary>
        /// Add or update an edge by id, name, type, source, target.
        /// Input: id, name, type (string), source, target (INode)
        /// Output: IEdge.
        /// Example: graph.AddOrUpdateEdge("e1", "Edge1", "calls", src, tgt)
        /// </summary>
        IEdge AddOrUpdateEdge(string id, string name, string type, INode source, INode target);

        /// <summary>
        /// Add or update an edge with attributes.
        /// Input: id, name, type (string), attributes (dict), source, target (INode)
        /// Output: IEdge.
        /// Example: graph.AddOrUpdateEdge("e1", "Edge1", "calls", attrs, src, tgt)
        /// </summary>
        IEdge AddOrUpdateEdge(string id, string name, string type, IReadOnlyDictionary<string, string> attributes, INode source, INode target);

        /// <summary>
        /// Add or update an edge object.
        /// Input: edge (IEdge)
        /// Output: IEdge.
        /// Example: graph.AddOrUpdateEdge(edge)
        /// </summary>
        IEdge AddOrUpdateEdge(IEdge edge);

        /// <summary>
        /// Remove a node.
        /// Input: node (INode)
        /// Output: true if removed.
        /// Example: graph.RemoveNode(node)
        /// </summary>
        bool RemoveNode(INode node);

        /// <summary>
        /// Remove an edge.
        /// Input: edge (IEdge)
        /// Output: true if removed.
        /// Example: graph.RemoveEdge(edge)
        /// </summary>
        bool RemoveEdge(IEdge edge);

        /// <summary>
        /// Validate graph for errors.
        /// Output: List of error strings.
        /// Example: var errors = graph.Validate()
        /// </summary>
        IEnumerable<string> Validate();

        /// <summary>
        /// Get nodes adjacent to a node.
        /// Input: node (INode)
        /// Output: IEnumerable of INode.
        /// Example: graph.GetAdjacentNodes(node)
        /// </summary>
        IEnumerable<INode> GetAdjacentNodes(INode node);

        /// <summary>
        /// Get predecessor nodes.
        /// Input: node (INode)
        /// Output: IEnumerable of INode.
        /// Example: graph.GetPredecessors(node)
        /// </summary>
        IEnumerable<INode> GetPredecessors(INode node);

        /// <summary>
        /// Get outgoing edges from a node.
        /// Input: node (INode)
        /// Output: IEnumerable of IEdge.
        /// Example: graph.GetOutgoingEdges(node)
        /// </summary>
        IEnumerable<IEdge> GetOutgoingEdges(INode node);

        /// <summary>
        /// Get incoming edges to a node.
        /// Input: node (INode)
        /// Output: IEnumerable of IEdge.
        /// Example: graph.GetIncomingEdges(node)
        /// </summary>
        IEnumerable<IEdge> GetIncomingEdges(INode node);

        /// <summary>
        /// Check if a path exists between two nodes.
        /// Input: source, target (INode)
        /// Output: true if path exists.
        /// Example: graph.HasPath(src, tgt)
        /// </summary>
        bool HasPath(INode source, INode target);

        /// <summary>
        /// Breadth-first traversal from a node.
        /// Input: startNode (INode)
        /// Output: IEnumerable of INode.
        /// Example: graph.BreadthFirstTraversal(start)
        /// </summary>
        IEnumerable<INode> BreadthFirstTraversal(INode startNode);

        /// <summary>
        /// Depth-first traversal from a node.
        /// Input: startNode (INode)
        /// Output: IEnumerable of INode.
        /// Example: graph.DepthFirstTraversal(start)
        /// </summary>
        IEnumerable<INode> DepthFirstTraversal(INode startNode);

        /// <summary>
        /// Get topological sort of nodes.
        /// Output: List of INode.
        /// Example: var sorted = graph.GetTopologicalSort()
        /// </summary>
        IReadOnlyList<INode> GetTopologicalSort();

        /// <summary>
        /// Find all paths between two nodes.
        /// Input: source, target (INode)
        /// Output: IEnumerable of node paths.
        /// Example: graph.FindAllPaths(src, tgt)
        /// </summary>
        IEnumerable<IEnumerable<INode>> FindAllPaths(INode source, INode target);
    }
}



```
