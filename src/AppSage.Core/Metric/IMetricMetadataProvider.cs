namespace AppSage.Core.Metric
{
    public interface IMetricMetadataProvider
    {
        IEnumerable<MetricMetadata> MetricInfo { get; }
    }
}
