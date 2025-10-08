namespace AppSage.Core.Metric
{
    public interface IMetricCollector
    {
        public const string TOOL_RUN_INFO = "AppSage.ToolRunInfo";
        void Add(IMetric metric);
        void CompleteAdding();

    }
}
