using AppSage.Core.Localization;

namespace AppSage.Providers.BasicRepositoryMetric
{
    public class ConstString : LocalizationManager
    {
        public ConstString() : base("AppSage.Providers.BasicRepositoryMetric") { }
       

        public static string UNKNOWN = "Unknown";
        public static string UNCATEGORIZED = "Uncategorized";


    }
}
