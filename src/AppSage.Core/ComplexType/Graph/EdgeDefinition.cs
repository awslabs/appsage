namespace AppSage.Core.ComplexType.Graph
{
    public delegate (string EdgeId, string EdgeName, string EdgeType, IReadOnlyDictionary<string, string> EdgeAttributes) EdgeDefinition(INode sourceNode, INode targetNode);
}
