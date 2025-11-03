### Complete Example: Interface Implementation Analysis - return a DataTable with analysis. 
```csharp

using AppSage.Core.ComplexType.Graph;
using System.Data;
using AppSage.Core.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppSage.MCPServer.SampleQuery
{
    public class MyQuery
    {


        /// <summary>
        /// Analyzes interface implementation patterns in a DirectedGraph and returns detailed assembly dependency information.
        /// This method specifically finds all classes that implement the ILogger interface and provides comprehensive
        /// analysis of their assembly dependencies, version conflicts, and logging framework usage across the codebase.
        /// </summary>
        /// <param name="graph">The DirectedGraph containing dependency information across repository, solution, project, assembly, and class levels</param>
        /// <returns>
        /// A list of DataTables containing:
        /// 1. ClassesImplementingILogger: Details of classes implementing ILogger interface with metrics
        /// 2. ClassAssemblyDependencies: Assembly dependencies for each implementing class with version info
        /// 3. AssemblyVersionConflicts: Summary of version conflicts across implementing classes (if any exist)
        /// 4. LoggingRelatedAssemblies: Overview of all logging frameworks used in the solution
        /// </returns>
        /// <remarks>
        /// This method demonstrates advanced graph traversal techniques for dependency analysis and can be adapted
        /// for analyzing other interface implementations. It handles both direct and indirect assembly dependencies
        /// through project references and provides version conflict detection for better dependency management.
        /// </remarks>
        public static IEnumerable<DataTable> Execute(IDirectedGraph graph)
        {
            var results = new List<DataTable>();

            // Find all classes that implement ILogger interface and analyze their assembly references
            var loggerInterface = graph.Nodes
                .FirstOrDefault(n => n.Type == "Interface" &&
                                (n.Name == "ILogger" || n.Id.Contains("Microsoft.Extensions.Logging.ILogger")));

            if (loggerInterface != null)
            {
                // Step 1: Find all classes that implement ILogger interface
                var implementingClasses = graph.GetPredecessors(loggerInterface)
                    .Where(n => n.Type == "Class")
                    .Where(n => graph.GetOutgoingEdges(n)
                        .Any(e => e.Type == "Implement" && e.Target.Equals(loggerInterface)))
                    .ToList();

                // Create DataTable 1: Classes implementing ILogger
                var classesTable = new DataTable("ClassesImplementingILogger");
                classesTable.Columns.Add("ClassName", typeof(string));
                classesTable.Columns.Add("ClassId", typeof(string));
                classesTable.Columns.Add("MethodCount", typeof(int));
                classesTable.Columns.Add("LinesOfCode", typeof(int));
                classesTable.Columns.Add("RepositoryName", typeof(string));

                foreach (var implementingClass in implementingClasses)
                {
                    var methodCount = implementingClass.Attributes.ContainsKey("NameTypeMethodCount")
                        ? int.Parse(implementingClass.Attributes["NameTypeMethodCount"]) : 0;
                    var linesOfCode = implementingClass.Attributes.ContainsKey("NameTypeLinesOfCode")
                        ? int.Parse(implementingClass.Attributes["NameTypeLinesOfCode"]) : 0;
                    var repositoryName = implementingClass.Attributes.ContainsKey("RepositoryName")
                        ? implementingClass.Attributes["RepositoryName"] : "Unknown";

                    classesTable.Rows.Add(
                        implementingClass.Name,
                        implementingClass.Id,
                        methodCount,
                        linesOfCode,
                        repositoryName
                    );
                }
                results.Add(classesTable);

                // Step 2: Create DataTable 2: Assembly dependencies for each class
                var assemblyDependenciesTable = new DataTable("ClassAssemblyDependencies");
                assemblyDependenciesTable.Columns.Add("ClassName", typeof(string));
                assemblyDependenciesTable.Columns.Add("ClassId", typeof(string));
                assemblyDependenciesTable.Columns.Add("AssemblyName", typeof(string));
                assemblyDependenciesTable.Columns.Add("AssemblyVersion", typeof(string));
                assemblyDependenciesTable.Columns.Add("DependencyType", typeof(string));

                foreach (var implementingClass in implementingClasses)
                {
                    // Find all assemblies referenced by this class (through various dependency types)
                    var assemblyDependencies = graph.GetAdjacentNodes(implementingClass)
                        .Where(n => n.Type == "Assembly")
                        .ToList();

                    // Also find assemblies referenced indirectly through projects
                    var projectDependencies = graph.GetAdjacentNodes(implementingClass)
                        .Where(n => n.Type == "Project");

                    foreach (var project in projectDependencies)
                    {
                        var projectAssemblies = graph.GetAdjacentNodes(project)
                            .Where(n => n.Type == "Assembly");
                        assemblyDependencies.AddRange(projectAssemblies);
                    }

                    // Add rows for each assembly dependency
                    foreach (var assembly in assemblyDependencies.Distinct())
                    {
                        string assemblyName = assembly.Attributes.ContainsKey("AssemblyName")
                            ? assembly.Attributes["AssemblyName"]
                            : assembly.Name;

                        string version = assembly.Attributes.ContainsKey("AssebmlyVersion")
                            ? assembly.Attributes["AssebmlyVersion"]
                            : "Unknown";

                        // Determine dependency type based on the edge
                        var edge = graph.GetOutgoingEdges(implementingClass)
                            .FirstOrDefault(e => e.Target.Equals(assembly));
                        string dependencyType = edge?.Type ?? "Indirect";

                        assemblyDependenciesTable.Rows.Add(
                            implementingClass.Name,
                            implementingClass.Id,
                            assemblyName,
                            version,
                            dependencyType
                        );
                    }
                }
                results.Add(assemblyDependenciesTable);

                // Step 3: Create DataTable 3: Assembly version conflicts summary
                var allAssemblyVersions = assemblyDependenciesTable.AsEnumerable()
                    .GroupBy(row => row.Field<string>("AssemblyName"))
                    .Where(g => g.Select(row => row.Field<string>("AssemblyVersion")).Distinct().Count() > 1)
                    .ToList();

                if (allAssemblyVersions.Any())
                {
                    var versionConflictsTable = new DataTable("AssemblyVersionConflicts");
                    versionConflictsTable.Columns.Add("AssemblyName", typeof(string));
                    versionConflictsTable.Columns.Add("ConflictingVersions", typeof(string));
                    versionConflictsTable.Columns.Add("AffectedClassCount", typeof(int));

                    foreach (var assemblyGroup in allAssemblyVersions)
                    {
                        var versions = assemblyGroup.Select(row => row.Field<string>("AssemblyVersion")).Distinct().ToList();
                        var affectedClasses = assemblyGroup.Select(row => row.Field<string>("ClassName")).Distinct().Count();

                        versionConflictsTable.Rows.Add(
                            assemblyGroup.Key,
                            string.Join(", ", versions),
                            affectedClasses
                        );
                    }
                    results.Add(versionConflictsTable);
                }
            }

            // Step 4: Create DataTable 4: All logging-related assemblies summary
            var loggingAssembliesTable = new DataTable("LoggingRelatedAssemblies");
            loggingAssembliesTable.Columns.Add("AssemblyName", typeof(string));
            loggingAssembliesTable.Columns.Add("AssemblyVersion", typeof(string));
            loggingAssembliesTable.Columns.Add("ReferencingProjectCount", typeof(int));

            var loggingAssemblies = graph.Nodes
                .Where(n => n.Type == "Assembly")
                .Where(n => n.Name.Contains("Microsoft.Extensions.Logging") ||
                            n.Name.Contains("Serilog") ||
                            n.Name.Contains("NLog") ||
                            n.Name.Contains("log4net"))
                .ToList();

            foreach (var assembly in loggingAssemblies)
            {
                string assemblyName = assembly.Attributes.ContainsKey("AssemblyName")
                    ? assembly.Attributes["AssemblyName"]
                    : assembly.Name;
                string version = assembly.Attributes.ContainsKey("AssebmlyVersion")
                    ? assembly.Attributes["AssebmlyVersion"]
                    : "Unknown";

                // Count how many projects reference this assembly
                var referencingProjects = graph.GetPredecessors(assembly)
                    .Where(n => n.Type == "Project")
                    .Count();

                loggingAssembliesTable.Rows.Add(
                    assemblyName,
                    version,
                    referencingProjects
                );
            }
            }
            results.Add(loggingAssembliesTable);

            return results;
        }
    }
}


```