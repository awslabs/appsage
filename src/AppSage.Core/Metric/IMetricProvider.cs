namespace AppSage.Core.Metric
{

    public interface IMetricProvider
    {
        string FullQualifiedName { get; }
        string Description { get; }

        bool Initialize();

        void Run(IMetricCollector collectorQueue);
    }
}
