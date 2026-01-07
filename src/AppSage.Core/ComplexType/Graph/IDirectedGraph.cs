using System.Collections.Generic;

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
        /// Remove a set of nodes.
        /// Input: set of node (INode)
        /// Output: true if at least one is removed.
        /// Example: graph.RemoveNode(nodes)
        /// </summary>
        bool RemoveNode(IEnumerable<INode> nodes);

        /// <summary>
        /// Remove an edge.
        /// Input: edge (IEdge)
        /// Output: true if removed.
        /// Example: graph.RemoveEdge(edges)
        /// </summary>
        bool RemoveEdge(IEdge edge);

        /// <summary>
        /// Remove a set of edges.
        /// Input: set of edges (IEdge)
        /// Output: true if at least one is removed.
        /// Example: graph.RemoveEdge(edges)
        /// </summary>
        bool RemoveEdge(IEnumerable<IEdge> edges);

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
