# DirectedGraph Dependency Analysis Documentation

This document describes the structure of the merged DirectedGraph (X) created by the `DotNetDependencyAnalysisProvider`. The graph represents dependencies across multiple levels: repository → solution → project → assembly → class → method.

## Important Notes

- **Namespace Filtering**: The provider supports configuration for including/excluding specific namespace prefixes through `NamespacePrefixToInclude` and `NamespacePrefixToExclude` settings.
- **Large Metrics**: Graphs exceeding configured thresholds for nodes (`LargeMetricDependencyGraphNodeThreshold`) or edges (`LargeMetricDependencyGraphEdgeThreshold`) are marked with `IsLargeMetric = true`.
- **Per-Project Graphs**: Each project generates its own dependency graph metric to avoid extremely large single graphs and enable easier filtering and aggregation.
- **Testing Filter**: The current implementation includes a temporary filter for types containing "Iris.Service.Company" - this should be removed in production.

## Node Types

### Repository
**Description**: Represents a source control repository containing one or more solutions/projects.
**Example**: `MyCompany.MainRepo`

### Solution
**Description**: Represents a Visual Studio solution file (.sln) containing multiple projects.
**Example**: `MyCompany.WebApplication.sln`

### Project
**Description**: Represents a .NET project (.csproj, .vbproj) containing source code and references.
**Example**: `MyCompany.WebAPI`, `MyCompany.Core`

### Assembly
**Description**: Represents compiled assemblies (DLLs) referenced by projects, including NuGet packages and framework assemblies.
**Example**: `System.Text.Json`, `Newtonsoft.Json`, `EntityFramework`

### Class
**Description**: Represents C# classes declared in the source code.
**Example**: `UserController`, `CustomerService`

### Interface
**Description**: Represents C# interfaces declared in the source code.
**Example**: `IUserRepository`, `IPaymentService`

### Struct
**Description**: Represents C# structures declared in the source code.
**Example**: `Point`, `Vector3`

### Enum
**Description**: Represents C# enumerations declared in the source code.
**Example**: `OrderStatus`, `PaymentMethod`

### Delegate
**Description**: Represents C# delegates and function pointers.
**Example**: `EventHandler`, `Action<T>`

### Generic
**Description**: Represents generic types with type parameters or instantiated generics.
**Example**: `List<Customer>`, `Dictionary<string, int>`

### Array
**Description**: Represents array types.
**Example**: `string[]`, `Customer[]`

### Method
**Description**: Represents C# methods declared in classes, interfaces, or structs.
**Example**: `GetUserById`, `SaveCustomer`

### Ambiguous
**Description**: Represents nodes that could belong to multiple categories or are not clearly defined during analysis.
**Example**: Unresolved types, error types from compilation issues

### Miscellaneous
**Description**: Fallback for any other type symbols not covered above.
**Example**: Type parameters, complex pointer types, function pointers



## Edge Types

### Reside
**Description**: Indicates containment/location relationship - where something physically resides.
**Example**: `ProjectNode → RepositoryNode` (project resides in repository)

### Refer
**Description**: Indicates reference relationships - one entity references another.
**Examples**: 
- `ProjectNode → ProjectNode` (project references another project)
- `ProjectNode → AssemblyNode` (project references assembly/NuGet package)
- `SolutionNode → ProjectNode` (solution references project)

### Inherit
**Description**: Indicates inheritance relationship - class inherits from base class.
**Example**: `CustomerController → BaseController`

### Implement
**Description**: Indicates interface implementation - class implements interface.
**Example**: `UserService → IUserService`

### Composition
**Description**: Indicates has-a relationship - class has field/property of another type.
**Example**: `OrderService → IUserRepository` (OrderService has IUserRepository field)

### Use
**Description**: Indicates usage as method parameter or return type.
**Example**: `UserController → UserDto` (method parameter/return type)

### Invoke
**Description**: Indicates method call dependency - one class invokes methods on another.
**Example**: `UserController → UserService` (controller calls service methods)

### Access
**Description**: Indicates property/field access dependency.
**Example**: `OrderService → Customer.Name` (accessing Customer properties)

### Create
**Description**: Indicates object instantiation dependency - using `new` keyword.
**Example**: `UserService → User` (service creates new User instances)

### Declare
**Description**: Indicates local variable declaration dependency.
**Example**: `PaymentService → PaymentResult` (declaring PaymentResult variables)

### Has
**Description**: Indicates general "has" relationship - one entity has another.
**Example**: `TypeNode → MethodNode` (type has method)

## Node Attributes

### RepositoryName
**Description**: Name of the repository containing the node.
**Example**: `"AppSage"`, `"MyCompany.Core"`
**Used on**: All node types

### ProjectClassCount
**Description**: Total number of classes in the project.
**Example**: `"42"`
**Used on**: Project nodes

### ProjectDataNameTypeCount
**Description**: Number of data/model classes in the project (classes with primarily properties) - alternative naming.
**Example**: `"15"`
**Used on**: Project nodes

### ProjectMethodCount
**Description**: Total number of methods in the project.
**Example**: `"298"`
**Used on**: Project nodes

### ProjectLinesOfCode
**Description**: Total lines of code in the project.
**Example**: `"5420"`
**Used on**: Project nodes

### ProjectLanguage
**Description**: Programming language used in the project.
**Example**: `"C#"`, `"Visual Basic"`
**Used on**: Project nodes

### ProjectTargetFramework
**Description**: Target .NET framework version for the project.
**Example**: `"net8.0"`, `"net6.0"`, `"netstandard2.0"`
**Used on**: Project nodes

### ProjectAssemblyName
**Description**: Assembly name of the project.
**Example**: `"MyCompany.WebAPI"`, `"MyCompany.Core"`
**Used on**: Project nodes

### ProjectType
**Description**: Type of the project (executable, library, etc.).
**Example**: `"Library"`, `"Executable"`, `"Web Application"`
**Used on**: Project nodes

### NameTypeMethodCount
**Description**: Number of methods in the class (alternative naming).
**Example**: `"8"`
**Used on**: Class, Interface, Struct nodes

### NameTypeLinesOfCode
**Description**: Lines of code in the class (alternative naming).
**Example**: `"156"`
**Used on**: Class, Interface, Struct nodes

### NameTypePosition
**Description**: Position information (line and character positions) of the type declaration in the source file.
**Example**: `"[15,4,45,5]"` (start line, start char, end line, end char)
**Used on**: Class, Interface, Struct, Enum nodes

### ResourceFilePath
**Description**: Path to the source file containing the type or resource.
**Example**: `"src/Controllers/UserController.cs"`
**Used on**: All node types

### AssemblyName
**Description**: Name of the assembly.
**Example**: `"System.Text.Json"`, `"EntityFramework"`
**Used on**: Assembly nodes

### AssemblyVersion (Note: typo in constant as "AssebmlyVersion")
**Description**: Version of the assembly.
**Example**: `"6.0.0.0"`, `"7.0.12"`
**Used on**: Assembly nodes

### AssemblyTargetFramework
**Description**: Target framework of the assembly.
**Example**: `"net8.0"`, `"netstandard2.0"`
**Used on**: Assembly nodes

### AssemblyPath
**Description**: File system path to the assembly.
**Example**: `"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.0\System.Text.Json.dll"`
**Used on**: Assembly nodes

## Usage Examples

### Example 1: Querying Project Dependencies
```csharp
// Find all projects that depend on a specific project
var targetProject = graph.Nodes.First(n => n.Type == "Project" && n.Name == "MyCompany.Core");
var dependentProjects = graph.GetPredecessors(targetProject)
    .Where(n => n.Type == "Project");
```

### Example 2: Finding Class Inheritance Chains
```csharp
// Find all classes that inherit from BaseController
var baseController = graph.Nodes.First(n => n.Type == "Class" && n.Name == "BaseController");
var derivedClasses = graph.GetPredecessors(baseController)
    .Where(n => n.Type == "Class");
```

### Example 3: Analyzing Assembly Usage
```csharp
// Find which projects use Entity Framework
var efAssembly = graph.Nodes.First(n => n.Type == "Assembly" && n.Name.Contains("EntityFramework"));
var projectsUsingEF = graph.GetPredecessors(efAssembly)
    .Where(n => n.Type == "Project");
```

### Example 4: Finding High-Complexity Classes
```csharp
// Find classes with high method count
var complexClasses = graph.Nodes
    .Where(n => n.Type == "Class")
    .Where(n => n.Attributes.ContainsKey("NameTypeMethodCount") && 
                int.Parse(n.Attributes["NameTypeMethodCount"]) > 20);
```

### Example 5: Analyzing Method Dependencies
```csharp
// Find all methods invoked by a specific class
var userController = graph.Nodes.First(n => n.Type == "Class" && n.Name == "UserController");
var invokedMethods = graph.GetOutgoingEdges(userController)
    .Where(e => e.Type == "Invoke" && e.Target.Type == "Method")
    .Select(e => e.Target);
```

### Example 6: Finding Generic Type Usage
```csharp
// Find all generic types used in the codebase
var genericTypes = graph.Nodes
    .Where(n => n.Type == "Generic");

// Find specific generic instantiations like List<T>
var listTypes = graph.Nodes
    .Where(n => n.Type == "Generic" && n.Name.StartsWith("List"));
```

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
        public static IEnumerable<DataTable> Execute(DirectedGraph graph)
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