namespace AppSage.Providers.DotNet
{
    public static class MetricName
    {
        public static class DotNet
        {
            public const string SOLUTION_COUNT = "DotNet.SolutionCount";
            public const string PROJECT_COUNT = "DotNet.ProjectCount";
            public const string PROJECT_TYPE_COUNT = "DotNet.ProjectTypeCount";

            public const string CLASS_COUNT = "DotNet.ClassCount";
            public const string METHOD_COUNT = "DotNet.MethodCount";
            public const string PROPERTY_COUNT = "DotNet.PropertyCount";
            public const string NAMESPACE_COUNT = "DotNet.NamespaceCount";
            public const string INTERFACE_COUNT = "DotNet.InterfaceCount";

            public const string SOLUTION_PROJECT_MAPPING = "DotNet.SolutionProjectMapping";

            public const string MERGED_CODE_DEPENDENCY_GRAPH = "DotNet.MergedCodeDependencyGraph";

            public const string AISummary = "DotNet.AISummary";
            public static class Project
            {
                public const string LANGUAGE = "DotNet.Project.Language";
                public const string TYPE = "DotNet.Project.Type";
                public const string DOCUMENT_COUNT_TOTAL = "DotNet.Project.DocumentCountTotal";
                public const string DOCUMENT_COUNT_PUREDOTNET = "DotNet.Project.DocumentCountPureDotNet";
                public const string DOCUMENT_COUNT_SCRIPTS = "DotNet.Project.DocumentCountScript";
                public const string NUGET_PACKAGES = "DotNet.Project.NugetPackages";
                public const string DOTNET_VERSION = "DotNet.Project.DotNetVersion";
                public const string CLASS_COUNT = "DotNet.Project.ClassCount";
                public const string DATA_CLASS_COUNT = "DotNet.Project.DataClassCount";
                public const string METHOD_COUNT = "DotNet.Project.MethodCount";

                public const string LINES_OF_CODE = "DotNet.Project.LinesOfCode";
                public const string CLASS_STATISTICS = "DotNet.Project.ClassStatistics";
                public const string METHOD_STATISTICS = "DotNet.Project.MethodStatistics";
                public const string CLASS_REFERENCE_USAGE_APROXIMATION = "DotNet.Project.ClassReferenceUsageApproximation";
                public const string PROJECT_REFERENCE_USAGE_APROXIMATION = "DotNet.Project.ProjectReferenceUsageApproximation";
                public const string PROJECT_LIBRARY_IMPACT_APPROXIMATION = "DotNet.Project.ProjectLibraryImpactApproximation";
                public const string PROJECT_LIBRARY_IMPACT_APPROXIMATION_EXPANDED = "DotNet.Project.ProjectLibraryImpactApproximationExpanded";

                public const string CODE_DEPENDENCY_GRAPH = "DotNet.Project.CodeDependencyGraph";

                public const string AISummary = "DotNet.Project.AISummary";
                public class Document
                {
                    public const string NAME = "DotNet.Project.Document.Name";
                    public const string TYPE = "DotNet.Project.Document.Type";
                    public const string LINES_OF_CODE = "DotNet.Project.Document.LinesOfCode";
                    public const string CLASS_COUNT = "DotNet.Project.Document.ClassCount";
                    public const string METHOD_COUNT = "DotNet.Project.Document.MethodCount";
                    public const string PROPERTY_COUNT = "DotNet.Project.Document.PropertyCount";
                    public const string AISummary = "DotNet.Project.Document.AISummary";
                }
            }
            public static class Nuget
            {
                public const string PACKAGE_COUNT = "DotNet.Nuget.PackageCount";
                public const string AISummary = "DotNet.Nuget.AISummary";
            }
            public static class CSharp
            {
                public const string ENUM_COUNT = "DotNet.CSharp.EnumCount";
            }
        }

    }
}
