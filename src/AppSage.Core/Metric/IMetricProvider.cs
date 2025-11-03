namespace AppSage.Core.Metric
{

    public interface IMetricProvider
    {
        string FullQualifiedName
        {
            get
            {
                return this.GetType().FullName;
            }
        }
        string Description { get; }
        void Run(IMetricCollector collectorQueue);
    }
}
