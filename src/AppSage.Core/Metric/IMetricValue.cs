namespace AppSage.Core.Metric
{
    public interface IMetricValue<T>:IMetric
    {
        T? Value { get; }
    }
}
