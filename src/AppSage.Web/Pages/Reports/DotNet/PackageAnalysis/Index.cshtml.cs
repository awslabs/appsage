using AppSage.Core.Configuration;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Web.Components.Filter;
using System.Data;

namespace AppSage.Web.Pages.Reports.DotNet.PackageAnalysis
{
    public class IndexModel : MetricFilterPageModel
    {
        public IndexModel(IMetricReader metricReader) : base(metricReader) { }
        public IndexViewModel Dashboard { get; set; } = new IndexViewModel();
        public override List<IMetric> GetMyMetrics()
        {
            string providerName = "AppSage.Providers.DotNet.BasicCodeAnalysis.DotNetBasicCodeAnalysisProvider";

            var allMetrics = GetAllMetrics();
            var result = allMetrics.Where(x => x.Provider == providerName).ToList();
            return result;
        }

        protected override void LoadData()
        {
            var metrics = GetFilteredMetrics();
            // FilterPossibleValue metrics that have a Resource property (implementing IResourceMetricValue interface)

            Dashboard.ReferenceApproximationMetricTableName = MetricName.DotNet.Project.PROJECT_LIBRARY_IMPACT_APPROXIMATION;



        }
    }
}
