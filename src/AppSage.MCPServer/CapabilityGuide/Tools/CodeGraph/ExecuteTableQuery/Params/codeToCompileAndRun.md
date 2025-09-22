```codeToCompileAndRun``` must have a .NET C# public class ```public class MyQuery``` with a  public static method with the following signature:
```public static IEnumerable<DataTable> Execute(IDirectedGraph graph)```

An example value for the parameter ```codeToCompileAndRun``` is given at the end at "codeToCompileAndRun Complete Example"


It's critical that the method signature is exactly as specified above and that the class is called ```MyQuery```.

Inside the method, you can use LINQ to query the graph and return one or more ```DataTables``` with the results of your analysis.
The code for the analysis must be written in C#, based on the what the user has requested.
You must explicityly import any namespaces you use in the code with using statement.
The ```codeToCompileAndRun``` must be self-contained C# content and must not depend on any external files or resources. It must be compilable.  

# DirectedGraph Dependency Analysis Documentation

This document describes the structure of the ```IDirectedGraph```. The graph represents dependencies across multiple levels: repository ,solution , project ,assembly ,class & method.

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

### Calculated
**Description**: Represents types derived from analysis of an existing graph, such as through LINQ projections.
**Example**: Any node created to keep track of Lines of code, method counts of a given set of classes that implement a certain interface.

### Miscellaneous
**Description**: Fallback for any other type symbols not covered above.
**Example**: Type parameters, complex pointer types



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

## Node Attributes

### RepositoryName
**Description**: Name of the repository containing the node.
**Example**: `"AppSage"`, `"MyCompany.Core"`
**Used on**: All node types

### ProjectClassCount
**Description**: Total number of classes in the project.
**Example**: `"42"`
**Used on**: Project nodes

### ProjectDataClassCount
**Description**: Number of data/model classes in the project (classes with primarily properties).
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

### ClassMethodCount
**Description**: Number of methods in the class.
**Example**: `"8"`
**Used on**: Class, Interface, Struct nodes

### ClassLinesOfCode
**Description**: Lines of code in the class.
**Example**: `"156"`
**Used on**: Class, Interface, Struct nodes

### MethodLinesOfCode
**Description**: Lines of code in a specific method.
**Example**: `"23"`
**Used on**: Method nodes and referenced in method-level dependencies

### AssemblyName
**Description**: Name of the assembly.
**Example**: `"System.Text.Json"`, `"EntityFramework"`
**Used on**: Assembly nodes

### AssemblyVersion (Note: typo in constant as "AssebmlyVersion")
**Description**: Version of the assembly.
**Example**: `"6.0.0.0"`, `"7.0.12"`
**Used on**: Assembly nodes

### AssemblyIsFramework
**Description**: Boolean indicating if assembly is a framework/system assembly.
**Example**: `"True"` (for System.Text.Json), `"False"` (for custom NuGet packages)
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
    .Where(n => n.Attributes.ContainsKey("ClassMethodCount") && 
                int.Parse(n.Attributes["ClassMethodCount"]) > 20);
```

### codeToCompileAndRun Complete Example

```csharp
//Interface Implementation Analysis - return a DataTable with analysis. 

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
        /// <param name="graph">The IDirectedGraph containing dependency information across repository, solution, project, assembly, and class levels</param>
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
                    var methodCount = implementingClass.Attributes.ContainsKey("ClassMethodCount")
                        ? int.Parse(implementingClass.Attributes["ClassMethodCount"]) : 0;
                    var linesOfCode = implementingClass.Attributes.ContainsKey("ClassLinesOfCode")
                        ? int.Parse(implementingClass.Attributes["ClassLinesOfCode"]) : 0;
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
                assemblyDependenciesTable.Columns.Add("IsFrameworkAssembly", typeof(bool));
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

                        bool isFramework = assembly.Attributes.ContainsKey("AssemblyIsFramework")
                            ? bool.Parse(assembly.Attributes["AssemblyIsFramework"])
                            : false;

                        // Determine dependency type based on the edge
                        var edge = graph.GetOutgoingEdges(implementingClass)
                            .FirstOrDefault(e => e.Target.Equals(assembly));
                        string dependencyType = edge?.Type ?? "Indirect";

                        assemblyDependenciesTable.Rows.Add(
                            implementingClass.Name,
                            implementingClass.Id,
                            assemblyName,
                            version,
                            isFramework,
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
            loggingAssembliesTable.Columns.Add("IsFrameworkAssembly", typeof(bool));
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
                bool isFramework = assembly.Attributes.ContainsKey("AssemblyIsFramework")
                    ? bool.Parse(assembly.Attributes["AssemblyIsFramework"])
                    : false;

                // Count how many projects reference this assembly
                var referencingProjects = graph.GetPredecessors(assembly)
                    .Where(n => n.Type == "Project")
                    .Count();

                loggingAssembliesTable.Rows.Add(
                    assemblyName,
                    version,
                    isFramework,
                    referencingProjects
                );
            }
            results.Add(loggingAssembliesTable);

            return results;
        }
    }
}


```