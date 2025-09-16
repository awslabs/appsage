using AppSage.Core.ComplexType.Graph;

public static class SampleGraphFilter2
{
    public static DirectedGraph Execute(DirectedGraph sourceGraph)
    {
        var result = new DirectedGraph();

        // Execute the user's query
        var filteredNodes = sourceGraph.Nodes.Where(n => n.Type == "Project");

        // Calculate graph-wide metrics for normalization
        var totalNodes = sourceGraph.Nodes.Count;
        var totalEdges = sourceGraph.Edges.Count;
        var nodesByType = sourceGraph.Nodes.GroupBy(n => n.Type).ToDictionary(g => g.Key, g => g.Count());

        // Process each filtered node and add calculated attributes
        foreach (var originalNode in filteredNodes)
        {
            // Clone the node to avoid modifying the original
            var enhancedNode = result.AddOrUpdateNode(originalNode.Id, originalNode.Name, originalNode.Type, originalNode.Attributes);

            // Calculate dependency metrics
            var incomingEdges = sourceGraph.GetIncomingEdges(originalNode).ToList();
            var outgoingEdges = sourceGraph.GetOutgoingEdges(originalNode).ToList();
            var adjacentNodes = sourceGraph.GetAdjacentNodes(originalNode).ToList();
            var predecessors = sourceGraph.GetPredecessors(originalNode).ToList();

            // Add calculated connectivity attributes
            enhancedNode.AddOrUpdateAttribute("InDegree", incomingEdges.Count.ToString());
            enhancedNode.AddOrUpdateAttribute("OutDegree", outgoingEdges.Count.ToString());
            enhancedNode.AddOrUpdateAttribute("TotalDegree", (incomingEdges.Count + outgoingEdges.Count).ToString());

            // Calculate dependency ratios
            var dependencyRatio = totalNodes > 1 ? (double)outgoingEdges.Count / (totalNodes - 1) : 0.0;
            var dependentRatio = totalNodes > 1 ? (double)incomingEdges.Count / (totalNodes - 1) : 0.0;
            enhancedNode.AddOrUpdateAttribute("DependencyRatio", dependencyRatio.ToString("F3"));
            enhancedNode.AddOrUpdateAttribute("DependentRatio", dependentRatio.ToString("F3"));

            // Calculate centrality measures
            var betweennessCentrality = CalculateBetweennessCentrality(originalNode, sourceGraph);
            enhancedNode.AddOrUpdateAttribute("BetweennessCentrality", betweennessCentrality.ToString("F3"));

            // Add type frequency information
            if (nodesByType.ContainsKey(originalNode.Type))
            {
                var typeFrequency = nodesByType[originalNode.Type];
                var typePercentage = (double)typeFrequency / totalNodes * 100;
                enhancedNode.AddOrUpdateAttribute("TypeFrequency", typeFrequency.ToString());
                enhancedNode.AddOrUpdateAttribute("TypePercentage", typePercentage.ToString("F1"));
            }

            // Calculate complexity score based on various factors
            var complexityScore = CalculateComplexityScore(originalNode, incomingEdges, outgoingEdges);
            enhancedNode.AddOrUpdateAttribute("ComplexityScore", complexityScore.ToString("F2"));

            // Add timestamp for when calculation was performed
            enhancedNode.AddOrUpdateAttribute("CalculatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Extract and enhance existing numeric attributes
            foreach (var attr in originalNode.Attributes)
            {
                if (int.TryParse(attr.Value, out int intValue))
                {
                    // Add normalized version of numeric attributes
                    var maxValue = sourceGraph.Nodes
                        .Where(n => n.Attributes.ContainsKey(attr.Key))
                        .Select(n => int.TryParse(n.Attributes[attr.Key], out int val) ? val : 0)
                        .DefaultIfEmpty(1)
                        .Max();

                    if (maxValue > 0)
                    {
                        var normalizedValue = (double)intValue / maxValue;
                        enhancedNode.AddOrUpdateAttribute(attr.Key + "_Normalized", normalizedValue.ToString("F3"));
                    }

                    // Add percentile ranking
                    var allValues = sourceGraph.Nodes
                        .Where(n => n.Attributes.ContainsKey(attr.Key))
                        .Select(n => int.TryParse(n.Attributes[attr.Key], out int val) ? val : 0)
                        .OrderBy(v => v)
                        .ToList();

                    if (allValues.Count > 0)
                    {
                        var percentile = (double)allValues.Count(v => v <= intValue) / allValues.Count * 100;
                        enhancedNode.AddOrUpdateAttribute(attr.Key + "_Percentile", percentile.ToString("F1"));
                    }
                }
            }
        }

        // Process edges and add calculated attributes
        var filteredNodeIds = new HashSet<string>(result.Nodes.Select(n => n.Id));
        var relevantEdges = sourceGraph.Edges.Where(e =>
            filteredNodeIds.Contains(e.Source.Id) &&
            filteredNodeIds.Contains(e.Target.Id));

        foreach (var originalEdge in relevantEdges)
        {
            var enhancedEdge = result.AddOrUpdateEdge(originalEdge);

            // Calculate edge importance metrics
            var sourceNode = result.Nodes.First(n => n.Id == originalEdge.Source.Id);
            var targetNode = result.Nodes.First(n => n.Id == originalEdge.Target.Id);

            var sourceOutDegree = int.Parse(sourceNode.Attributes.GetValueOrDefault("OutDegree", "0"));
            var targetInDegree = int.Parse(targetNode.Attributes.GetValueOrDefault("InDegree", "0"));

            // Edge weight based on node importance
            var edgeWeight = 1.0 / (sourceOutDegree + 1) + 1.0 / (targetInDegree + 1);
            enhancedEdge.AddOrUpdateAttribute("Weight", edgeWeight.ToString("F3"));

            // Edge type frequency
            var edgeTypeCount = sourceGraph.Edges.Count(e => e.Type == originalEdge.Type);
            var edgeTypePercentage = (double)edgeTypeCount / totalEdges * 100;
            enhancedEdge.AddOrUpdateAttribute("TypeFrequency", edgeTypeCount.ToString());
            enhancedEdge.AddOrUpdateAttribute("TypePercentage", edgeTypePercentage.ToString("F1"));

            // Add path information
            var pathExists = sourceGraph.HasPath(sourceNode, targetNode);
            enhancedEdge.AddOrUpdateAttribute("HasPath", pathExists.ToString());

            enhancedEdge.AddOrUpdateAttribute("CalculatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        // Add graph-level metadata as attributes to a special metadata node
        var metadataNode = result.AddOrUpdateNode("_METADATA_", "Graph Metadata", "METADATA");
        metadataNode.AddOrUpdateAttribute("OriginalNodeCount", totalNodes.ToString());
        metadataNode.AddOrUpdateAttribute("OriginalEdgeCount", totalEdges.ToString());
        metadataNode.AddOrUpdateAttribute("FilteredNodeCount", result.Nodes.Count(n => n.Type != "METADATA").ToString());
        metadataNode.AddOrUpdateAttribute("FilteredEdgeCount", result.Edges.Count.ToString());
        metadataNode.AddOrUpdateAttribute("FilterEfficiency",
            totalNodes > 0 ? ((double)result.Nodes.Count / totalNodes * 100).ToString("F1") : "0");
        metadataNode.AddOrUpdateAttribute("ProcessedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        return result;
    }

    private static double CalculateBetweennessCentrality(INode node, DirectedGraph graph)
    {
        // Simplified betweenness centrality calculation
        var nodeId = node.Id;
        double centrality = 0.0;
        var allNodes = graph.Nodes.Where(n => n.Id != nodeId).ToList();

        foreach (var source in allNodes)
        {
            foreach (var target in allNodes.Where(n => n.Id != source.Id))
            {
                var paths = graph.FindAllPaths(source, target).ToList();
                if (paths.Any())
                {
                    var pathsThroughNode = paths.Count(path => path.Any(n => n.Id == nodeId));
                    centrality += (double)pathsThroughNode / paths.Count;
                }
            }
        }

        return centrality;
    }

    private static double CalculateComplexityScore(INode node, List<IEdge> incomingEdges, IList<IEdge> outgoingEdges)
    {
        // Custom complexity calculation based on multiple factors
        var baseComplexity = 1.0;

        // Factor in degree centrality
        var degreeFactor = Math.Log10(incomingEdges.Count + outgoingEdges.Count + 1);

        // Factor in edge type diversity
        var edgeTypes = incomingEdges.Concat(outgoingEdges).Select(e => e.Type).Distinct().Count();
        var diversityFactor = Math.Log10(edgeTypes + 1);

        // Factor in node attributes (if they indicate complexity)
        var attributeFactor = 1.0;
        if (node.Attributes.ContainsKey("ClassMethodCount") &&
            int.TryParse(node.Attributes["ClassMethodCount"], out int methodCount))
        {
            attributeFactor = Math.Log10(methodCount + 1);
        }

        return baseComplexity + degreeFactor + diversityFactor + attributeFactor;
    }
}