using AppSage.Core.ComplexType.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppSage.MCPServer.SampleQuery
{
    internal class SampleGraphFilter4
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
