namespace AppSage.Providers.DotNet
{
    public static class MetricName
    {
        public static class DotNet
        {
            // Used in DotNetDependencyAnalysisProvider
            public const string SOLUTION_PROJECT_MAPPING = "DotNet.SolutionProjectMapping";

            // Used in DotNetDependencyAnalysisProvider - this is the merged graph
            public const string MERGED_CODE_DEPENDENCY_GRAPH = "DotNet.MergedCodeDependencyGraph";

            // Used in DotNetAIAnalysisProvider
            public const string AISummary = "DotNet.AISummary";

            public static class Project
            {
                // Used in DotNetBasicCodeAnalysisProvider
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

                // Used in DotNetDependencyAnalysisProvider  
                public const string CODE_DEPENDENCY_GRAPH = "DotNet.Project.CodeDependencyGraph";

                // Used in DotNetAIAnalysisProvider
                public const string AISummary = "DotNet.Project.AISummary";

                public class Document
                {
                    // Used in DotNetAIAnalysisProvider
                    public const string AISummary = "DotNet.Project.Document.AISummary";
                }
            }

            public static class Nuget
            {
                // Used in DotNetAIAnalysisProvider
                public const string AISummary = "DotNet.Nuget.AISummary";
            }
        }
    }
}
