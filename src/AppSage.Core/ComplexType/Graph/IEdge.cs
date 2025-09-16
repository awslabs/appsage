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
