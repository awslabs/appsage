# Example 1
The Execute method performs database dependency analysis on a DirectedGraph to identify classes that interact with databases and assess the complexity of migrating to a different database technology. Here's what it does:

Main Purpose:
Analyzes database dependencies in a codebase and creates a filtered graph containing only database-related components with migration complexity metrics.

Step-by-Step Process:
Identifies Database Classes (Y)

Scans all classes in the source graph
Finds classes that directly interact with database-related namespaces (Entity Framework, SQL Client, MySQL, Oracle, MongoDB, etc.)
Checks both the class namespace and its dependencies
- Finds Indirect Dependent Classes (Z)

Identifies classes that invoke or depend on the database classes
Creates a mapping of which database classes each indirect class connects to
Represents "one level up" dependencies that would be affected by database changes
- Adds Nodes with Enhanced Attributes

Database Classes: Marked as "Direct" database-related
Indirect Classes: Marked as "Indirect" with calculated MyComplexityLevel (average method count of connected database classes)
Database Methods: All methods from database classes with lines of code tracking
- Creates Migration Metadata

Generates a special "MetaData" node with comprehensive migration analysis
Calculates complexity metrics: total classes, methods, lines of code
Computes migration complexity score and impact radius
Provides migration approach recommendation (Direct/Careful Planning/Phased)
- Preserves Relationships

Connects indirect classes to their database dependencies
Links database classes to their methods
Associates metadata with all analyzed database classes
Output:
A filtered DirectedGraph containing only database-related components with enriched metadata for assessing the impact and complexity of database technology migration.

Key Value: Helps architects and developers understand the "blast radius" of changing database technologies and plan migration strategies accordingly.

```csharp

using AppSage.Core.ComplexType.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppSage.MCPServer.SampleQuery
{
    internal class MyQuery
    {
        private static readonly HashSet<string> DatabaseNamespaces = new HashSet<string>
        {
            "System.Data",
            "System.Data.SqlClient",
            "System.Data.Common",
            "Microsoft.Data.SqlClient",
            "Microsoft.EntityFrameworkCore",
            "EntityFramework",
            "Oracle.DataAccess",
            "Oracle.ManagedDataAccess",
            "MySql.Data",
            "Npgsql",
            "MongoDB.Driver",
            "Redis.StackExchange",
            "Cassandra",
            "Neo4j.Driver"
        };

        public static DirectedGraph Execute(DirectedGraph sourceGraph)
        {
            var result = new DirectedGraph();

            // Step 1: Find all classes Y that directly interact with database-related namespaces
            var databaseClasses = new List<INode>();
            
            foreach (var node in sourceGraph.Nodes.Where(n => n.Type == "Class"))
            {
                bool isDatabaseRelated = false;
                
                // Check node ID for database namespace
                if (DatabaseNamespaces.Any(dbNs => node.Id.Contains(dbNs)))
                {
                    isDatabaseRelated = true;
                }
                
                // Check dependencies for database-related assemblies or types
                if (!isDatabaseRelated)
                {
                    var dependencies = sourceGraph.GetAdjacentNodes(node);
                    isDatabaseRelated = dependencies.Any(dependency =>
                        (dependency.Type == "Assembly" || dependency.Type == "Class" || dependency.Type == "Interface") &&
                        DatabaseNamespaces.Any(dbNs => dependency.Id.Contains(dbNs) || dependency.Name.Contains(dbNs)));
                }
                
                if (isDatabaseRelated)
                {
                    databaseClasses.Add(node);
                }
            }

            // Step 2: Find classes Z that invoke database classes X (one level of indirect dependencies)
            var indirectDependentClasses = new List<INode>();
            var classToDbClassMap = new Dictionary<INode, List<INode>>();
            
            foreach (var dbClass in databaseClasses)
            {
                var callers = sourceGraph.GetPredecessors(dbClass)
                    .Where(n => n.Type == "Class" && !databaseClasses.Contains(n));

                foreach (var caller in callers)
                {
                    if (!indirectDependentClasses.Contains(caller))
                    {
                        indirectDependentClasses.Add(caller);
                    }

                    if (!classToDbClassMap.ContainsKey(caller))
                    {
                        classToDbClassMap[caller] = new List<INode>();
                    }

                    if (!classToDbClassMap[caller].Contains(dbClass))
                    {
                        classToDbClassMap[caller].Add(dbClass);
                    }
                }
            }

            // Step 3: Add database classes to result
            foreach (var dbClass in databaseClasses)
            {
                var newNode = result.AddOrUpdateNode(dbClass.Id, dbClass.Name, dbClass.Type, dbClass.Attributes);
                newNode.AddOrUpdateAttribute("DatabaseRelated", "Direct");
            }

            // Step 4: Add indirect dependent classes with complexity metrics
            foreach (var indirectClass in indirectDependentClasses)
            {
                var connectedDbClasses = classToDbClassMap[indirectClass];
                
                // Calculate MyComplexityLevel as average method count of connected database classes
                int totalMethodCount = 0;
                int classesWithMethodCount = 0;

                foreach (var dbClass in connectedDbClasses)
                {
                    if (dbClass.Attributes.ContainsKey("NameTypeMethodCount") &&
                        int.TryParse(dbClass.Attributes["NameTypeMethodCount"], out int methodCount))
                    {
                        totalMethodCount += methodCount;
                        classesWithMethodCount++;
                    }
                }

                double avgMethodCount = classesWithMethodCount > 0 ? (double)totalMethodCount / classesWithMethodCount : 0;

                var newNode = result.AddOrUpdateNode(indirectClass.Id, indirectClass.Name, indirectClass.Type, indirectClass.Attributes);
                newNode.AddOrUpdateAttribute("DatabaseRelated", "Indirect");
                newNode.AddOrUpdateAttribute("MyComplexityLevel", avgMethodCount.ToString("F2"));
                newNode.AddOrUpdateAttribute("ConnectedDatabaseClasses", connectedDbClasses.Count.ToString());
            }

            // Step 5: Add database-related methods with their lines of code
            var databaseMethods = new List<INode>();
            
            foreach (var dbClass in databaseClasses)
            {
                var methods = sourceGraph.GetAdjacentNodes(dbClass)
                    .Where(n => n.Type == "Method");

                foreach (var method in methods)
                {
                    var newMethodNode = result.AddOrUpdateNode(method.Id, method.Name, method.Type, method.Attributes);
                    newMethodNode.AddOrUpdateAttribute("DatabaseRelated", "Method");

                    // Ensure lines of code is tracked
                    if (!method.Attributes.ContainsKey("MethodLinesOfCode") && method.Attributes.ContainsKey("NameTypeLinesOfCode"))
                    {
                        newMethodNode.AddOrUpdateAttribute("MethodLinesOfCode", method.Attributes["NameTypeLinesOfCode"]);
                    }

                    databaseMethods.Add(newMethodNode);
                }
            }

            // Step 6: Create migration metadata
            var migrationMetadata = result.AddOrUpdateNode("DatabaseMigrationMetadata", "Database Migration Analysis", "MetaData");

            int totalDbClasses = databaseClasses.Count;
            int totalIndirectClasses = indirectDependentClasses.Count;
            int totalDbMethods = databaseMethods.Count;

            int totalDbLinesOfCode = CalculateTotalLinesOfCode(databaseClasses);
            int totalIndirectLinesOfCode = CalculateTotalLinesOfCode(indirectDependentClasses);

            // Migration complexity score (higher means more complex to migrate)
            double complexityScore = (totalDbClasses * 2.0) + (totalIndirectClasses * 1.0) + (totalDbMethods * 0.5);
            double impactRadius = totalDbClasses > 0 ? (double)totalIndirectClasses / totalDbClasses : 0;

            migrationMetadata.AddOrUpdateAttribute("TotalDatabaseClasses", totalDbClasses.ToString());
            migrationMetadata.AddOrUpdateAttribute("TotalIndirectDependentClasses", totalIndirectClasses.ToString());
            migrationMetadata.AddOrUpdateAttribute("TotalDatabaseMethods", totalDbMethods.ToString());
            migrationMetadata.AddOrUpdateAttribute("TotalDatabaseLinesOfCode", totalDbLinesOfCode.ToString());
            migrationMetadata.AddOrUpdateAttribute("TotalIndirectLinesOfCode", totalIndirectLinesOfCode.ToString());
            migrationMetadata.AddOrUpdateAttribute("MigrationComplexityScore", complexityScore.ToString("F2"));
            migrationMetadata.AddOrUpdateAttribute("ImpactRadius", impactRadius.ToString("F2"));
            migrationMetadata.AddOrUpdateAttribute("RecommendedMigrationApproach", GetMigrationApproach(complexityScore));

            // Step 7: Add relationships
            
            // Add edges from indirect classes to database classes
            foreach (var indirectClass in indirectDependentClasses)
            {
                var connectedDbClasses = classToDbClassMap[indirectClass];
                var sourceNode = result.GetNode(indirectClass.Id);

                foreach (var dbClass in connectedDbClasses)
                {
                    var targetNode = result.GetNode(dbClass.Id);
                    if (sourceNode != null && targetNode != null)
                    {
                        result.AddOrUpdateEdge(sourceNode, targetNode, "DatabaseDependency");
                    }
                }
            }

            // Add edges from database classes to methods
            foreach (var dbClass in databaseClasses)
            {
                var dbClassNode = result.GetNode(dbClass.Id);
                if (dbClassNode != null)
                {
                    var methods = sourceGraph.GetAdjacentNodes(dbClass).Where(n => n.Type == "Method");
                    foreach (var method in methods)
                    {
                        var methodNode = result.GetNode(method.Id);
                        if (methodNode != null)
                        {
                            result.AddOrUpdateEdge(dbClassNode, methodNode, "Has");
                        }
                    }
                }
            }

            // Connect metadata to all database classes
            foreach (var dbClass in databaseClasses)
            {
                var dbClassNode = result.GetNode(dbClass.Id);
                if (dbClassNode != null)
                {
                    result.AddOrUpdateEdge(migrationMetadata, dbClassNode, "Analyzes");
                }
            }

            return result;
        }

        private static int CalculateTotalLinesOfCode(List<INode> classes)
        {
            int totalLinesOfCode = 0;
            foreach (var classNode in classes)
            {
                if (classNode.Attributes.ContainsKey("NameTypeLinesOfCode") &&
                    int.TryParse(classNode.Attributes["NameTypeLinesOfCode"], out int loc))
                {
                    totalLinesOfCode += loc;
                }
            }
            return totalLinesOfCode;
        }

        private static string GetMigrationApproach(double complexityScore)
        {
            return complexityScore switch
            {
                > 100 => "Phased Migration",
                > 50 => "Careful Planning Required",
                _ => "Direct Migration Possible"
            };
        }
    }
}


```

# Example 2

Purpose
This method takes a DirectedGraph representing .NET project dependencies and creates an enhanced, filtered version that focuses on Project nodes with additional calculated metrics.

Main Operations
1. Node Filtering & Enhancement
Filters the input graph to only include nodes of type "Project"
For each project node, it calculates and adds several new metrics:
Dependency ratios: How connected each project is relative to the total graph
Complexity score: A custom metric based on edge count, edge type diversity, and method count
Normalized attributes: Converts numeric attributes (like line counts) to normalized 0-1 values
Timestamps: Records when calculations were performed
2. Edge Processing
Preserves edges between the filtered project nodes
Enhances each edge with additional metadata:
Weight: Based on the importance of connected nodes
Type frequency: How common that edge type is in the original graph
Path information: Whether a path exists between nodes
Timestamps: When edge calculations were done
3. Graph Metadata
Creates a special metadata node (_METADATA_) that stores:
Original vs filtered node/edge counts
Filter efficiency percentage
Processing timestamp
Key Use Cases
This appears designed for project dependency analysis where you want to:

Focus specifically on project-to-project relationships
Add quantitative metrics for complexity analysis
Normalize values for comparison across different sized codebases
Track when analysis was performed
The result is a more analytical version of the dependency graph that would be useful for architectural analysis, identifying overly complex projects, or understanding project interconnectedness in a .NET solution.

```csharp

using AppSage.Core.ComplexType.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MyQuery
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


            // Calculate dependency ratios
            var dependencyRatio = totalNodes > 1 ? (double)outgoingEdges.Count / (totalNodes - 1) : 0.0;
            var dependentRatio = totalNodes > 1 ? (double)incomingEdges.Count / (totalNodes - 1) : 0.0;
            enhancedNode.AddOrUpdateAttribute("DependencyRatio", dependencyRatio.ToString("F3"));
            enhancedNode.AddOrUpdateAttribute("DependentRatio", dependentRatio.ToString("F3"));


            // Calculate custom complexity score based on various factors
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

```