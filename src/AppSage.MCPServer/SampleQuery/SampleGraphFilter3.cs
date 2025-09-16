using AppSage.Core.ComplexType.Graph;

namespace AppSage.MCPServer.SampleQuery
{
    internal class SampleGraphFilter3
    {
        public static DirectedGraph Execute(DirectedGraph sourceGraph)
        {
            var result=DirectedGraph.MergeGraph(new List<DirectedGraph> { sourceGraph });

            var aggregateNode= result.AddOrUpdateNode("Aggregate", "Aggregate Node", "Aggregate");
            aggregateNode.AddOrUpdateAttribute("Description", "This node aggregates all other nodes in the graph.");
            aggregateNode.AddOrUpdateAttribute("NodeCount", sourceGraph.Nodes.Count.ToString());
            aggregateNode.AddOrUpdateAttribute("EdgeCount", sourceGraph.Edges.Count.ToString());
            aggregateNode.AddOrUpdateAttribute("CalculatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return result;
        }
    }
}
