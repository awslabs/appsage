namespace AppSage.Core.Metric
{
    public interface IMetricCollector
    {
        void Add(IMetric metric);
        void CompleteAdding();

    }
}
