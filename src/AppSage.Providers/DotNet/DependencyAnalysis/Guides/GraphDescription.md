# DirectedGraph Dependency Analysis Documentation

This document describes the structure of the merged DirectedGraph (X) created by the `DotNetDependencyAnalysisProvider`. The graph represents dependencies across multiple levels

# Important Notes

## Node Id
Each node has a unique identifier (`Node.Id`). If it is a .NET language concept like (classes, interfaces, structs,methods, record etc),
Node Id will be a fully qualified name (FQN) including the full namespace but excluding the assembly information. For the following code fragment:
   ```csharp
   namespace MyCompany.Services
   {
       public class UserService { public int Age=32; public string GetName(){return "bingo";}}
   }
   ```

The `Node.Id` for `UserService` will be `MyCompany.Services.UserService`
The `Node.Id` for `GetName` method will be `MyCompany.Services.UserService.GetName`.
The `Node.Id` for `Age` field will be `MyCompany.Services.UserService.Age`.
You can distinguish between different node types using the `Node.Type` property.

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
// Find classes with high method count. 20 is just an example.
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

