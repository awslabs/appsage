using Newtonsoft.Json;

namespace AppSage.Core.Metric
{
    public class ResourceMetricValue<T> : MetricValue<T>, IResourceMetricValue<T>
    {
        public string Resource { get; set; }

        [JsonConstructor]
        public ResourceMetricValue(string name, string resource) : base(name)
        {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }
        public ResourceMetricValue(string name, string resource, T? value) : base(name, value)
        {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }
        public ResourceMetricValue(string name, string provider, string resource) : base(name, provider)
        {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }
        public ResourceMetricValue(string name, string provider, string resource, T? value) : base(name, provider, value)
        {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }
   


        public ResourceMetricValue(string name, string provider,string segment, string resource, T? value) : base(name,provider,segment,value)
        {
            this.Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }
    }
     
}
