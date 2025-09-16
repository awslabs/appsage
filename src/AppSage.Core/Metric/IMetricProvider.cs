namespace AppSage.Core.Metric
{

    public interface IMetricProvider
    {
        string FullQualifiedName { get; }
        void Run(IMetricCollector collectorQueue);
    }
}
