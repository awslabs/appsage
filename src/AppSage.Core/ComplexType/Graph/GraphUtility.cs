namespace AppSage.Core.ComplexType.Graph
{
    public static class GraphUtility
    {
        public static string GetEdgeId(INode source, INode target, string edgeType)
        {
            return $"{source.Id}>[{edgeType}]>{target.Id}";
        }

        public static string GetEdgeName(INode source, INode target, string edgeName = "")
        {
            return $"{source.Name}>[{edgeName}]>{target.Name}";
        }
    }
}
