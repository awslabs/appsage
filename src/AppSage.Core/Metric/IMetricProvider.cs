namespace AppSage.Core.Metric
{

    public interface IMetricProvider
    {
        string FullQualifiedName { get; }
        string Description { get; }
        void Run(IMetricCollector collectorQueue);
    }
}
