namespace AppSage.Core.Metric
{
    public interface IResourceMetricValue<T> : IMetricValue<T>
    {
        /// <summary>
        /// The resource (E.g. repository, file, VM, etc.) that the metric is associated with.
        /// </summary>
        string Resource { get; }
    }
}
