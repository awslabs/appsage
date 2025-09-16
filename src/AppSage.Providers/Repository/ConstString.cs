using AppSage.Core.Localization;

namespace AppSage.Providers.Repository
{
    public class ConstString : LocalizationManager
    {
        public ConstString() : base("AppSage.Providers.Repository.Resources.Localization") { }
       

        public static string UNKNOWN = "Unknown";
        public static string UNCATEGORIZED = "Uncategorized";
        public static string REPOSITORY = "Repository";
        public static string BRANCH = "Branch";
        public static string COMMIT = "Commit";
        public static string AUTHOR = "Author";
        public static string DATE = "Date";
        public static string LAST_MODIFIED = "Last Modified";
        public static string TOTAL_COMMITS = "Total Commits";
        public static string TOTAL_BRANCHES = "Total Branches";
        public static string CONTRIBUTORS = "Contributors";


        public static class CommitInfo
        {
            public static string HASH = "Hash";
            public static string MESSAGE = "Message";
            public static string TIMESTAMP = "Timestamp";
            public static string COMMITTER = "Committer";
            public static string FILES_CHANGED = "Files Changed";
            public static string INSERTIONS = "Insertions";
            public static string DELETIONS = "Deletions";
            
        }
        
        public static class RepositoryMetrics
        {
            public static string ACTIVITY = "Activity";
            public static string CHURN = "Churn";
            public static string COMPLEXITY = "Complexity";
            public static string HOTSPOTS = "Hotspots";
            public static string CONTRIBUTION = "Contribution";
            
        }
    }
}
