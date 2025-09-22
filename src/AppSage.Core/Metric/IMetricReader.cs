namespace AppSage.Core.Metric
{
    public interface IMetricReader
    {
        IEnumerable<IMetric> GetMetricSet();
    }
}
