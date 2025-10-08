using AppSage.Core.Localization;

namespace AppSage.Providers.DotNet
{
    public class ConstString : LocalizationManager
    {
        public ConstString() : base("AppSage.Providers.DotNet.Resources.Localization") { }

        public static string UNKNOWN = "Unknown";
        public static string UNDEFINED = "Undefined";
        public static string UNCATEGORIZED = "Uncategorized";
        public static string OTHER = "Other";
        public static string FRAMEWORK = "Framework";
        public static string AGGREGATE = "AGGREGATE";

        public static class Dependency
        {
            public static class Attributes
            {
                public static string ResourceFilePath = "ResourceFilePath";

                public static string RepositoryName = "RepositoryName";
                
                public static string ProjectClassCount = "ProjectClassCount";
                public static string ProjectDataNameTypeCount = "ProjectDataNameTypeCount";
                public static string ProjectMethodCount = "ProjectMethodCount";
                public static string ProjectLinesOfCode = "ProjectLinesOfCode";
                public static string ProjectLanguage = "ProjectLanguage";
                public static string ProjectTargetFramework = "ProjectTargetFramework";
                public static string ProjectAssemblyName = "ProjectAssemblyName";
                public static string ProjectType = "ProjectType";

                public static string NameTypePosition= "NameTypePosition";
                public static string NameTypeMethodCount = "NameTypeMethodCount";
                public static string NameTypeLinesOfCode = "NameTypeLinesOfCode";


                public static string MethodLinesOfCode = "MethodLinesOfCode";


                public static string AssemblyName = "AssemblyName";
                public static string AssemblyVersion = "AssebmlyVersion";
                public static string AssemblyTargetFramework = "AssemblyTargetFramework";
                public static string AssemblyPath = "AssemblyPath";

         
            }

            public static class NodeType {
                public static string REPOSITORY = "Repository"; // e.g. a code repository or solution
                public static string SOLUTION = "Solution"; // e.g. a Visual Studio solution, a high level container for projects
                public static string PROJECT = "Project"; // e.g. a Visual Studio project, a container for code files and resources
                public static string CLASS = "Class"; // e.g. a C# class, a blueprint for creating objects

                public static string NAMESPACE = "Namespace"; // e.g. a C# namespace, a container for classes and other types
                public static string ASSEMBLY = "Assembly"; // e.g. a .NET assembly, a compiled code library (DLL or EXE)
                public static string MISCELLANEOUS = "Miscellaneous"; // e.g. files that do not fit into other categories, such as configuration files or documentation

                public static string STRUCT = "Struct"; // e.g. a C# struct, a value type that can contain data and methods
                public static string ENUM = "Enum"; // e.g. a C# enum, a distinct value type that consists of a set of named constants

                public static string INTERFACE = "Interface"; // e.g. a C# interface, a contract that defines a set of methods and properties without implementation
                public static string DELEGATE = "Delegate"; // e.g. a C# delegate, a type that represents references to methods with a specific parameter list and return type
                public static string ARRAY = "Array"; // e.g. a C# array, a collection of elements of the same type
                public static string GENERIC = "Generic"; // e.g. a C# generic type, a type that is parameterized over types
                public static string METHOD = "Method"; // e.g. a C# method, a function that is associated with a class or struct
                public static string EXTERNAL_REFERENCE = "ExternalReference"; // e.g. a reference to an external library or package, such as a NuGet package or a third-party DLL

                public static string AMBIGUOUS = "Ambiguous"; // e.g. a node that could belong to multiple categories or is not clearly defined
            }
            public static class DependencyType
            {
                public static string RESIDE = "Reside"; // e.g. residing in the same namespace or project
                public static string REFER= "Refer"; // e.g. referencing another class, method, or assembly
                public static string HAS = "Has"; // e.g. having a field or property of another type
                public static string IMPLEMENT = "Implement"; // e.g. implementing an interface
                public static string INHERIT = "Inherit"; //is-a relationship
                public static string COMPOSITION = "Composition";//has-a relationship
                public static string USE = "Use"; // e.g. using as a parameter in a method or return type of a method

                public static string INVOKE = "Invoke"; // e.g. invoking a method
                public static string ACCESS = "Access"; // e.g. accessing a property or field
                public static string CREATE = "Create"; // e.g. creating an instance of a class
                public static string DECLARE = "Declare"; // e.g. declaring a variable 
            }
        }

        public static class ExternalReference
        {
            public static class Attributes
            {
                public static string ProjectPath = "ProjectPath";
                public static string ProjectAssemblyName = "ProjectAssemblyName";
                public static string ReferenceName = "ReferenceName";
                public static string ReferenceVersion = "ReferenceVersion";
                public static string ReferenceCategory = "ReferenceCategory";
                public static string ReferenceType = "ExternalReference Type";
            }

            public static class ReferenceType
            {
                public static string DLL = "DLL";
                public static string NUGET = "NuGet";
                public static string PROJECT = "Project";
            }
        }

        public static class ProjectInfo
        {
            public static string ProjectPath = "ProjectPath";
            public static string ProjectAssemblyName = "ProjectAssemblyName";
            public static string ReferenceUsage= "ReferenceUsage";
        }

        public static class ClassInfo
        {
            public static string ProjectPath = "ProjectPath";
            public static string ProjectAssemblyName = "ProjectAssemblyName";
            public static string ClassName = "ClassName";
            public static string ClassMethodCount = "NameTypeMethodCount";
            public static string ClassLinesOfCode = "NameTypeLinesOfCode";
            public static string IsClassAffectedByDB = "IsClassAffectedByDB";
            public static string ClassReferenceNamespaces = "ClassReferenceNamespaces";
        }

        public static class MethodInfo
        {
            public static string ProjectPath = "ProjectPath";
            public static string ProjectAssemblyName = "ProjectAssemblyName";
            public static string ClassName = "ClassName";
            public static string ClassMethodCount = "NameTypeMethodCount";
            public static string ClassLinesOfCode = "NameTypeLinesOfCode";
            public static string IsClassAffectedByDB = "IsClassAffectedByDB";
            public static string ClassReferenceNamespaces = "ClassReferenceNamespaces";
            public static string MethodName = "MethodName";
            public static string MethodLinesOfCode = "MethodLinesOfCode";
            public static string MethodMaxParameters = "MethodMaxParameters";
        }
    }
}
