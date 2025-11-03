using Newtonsoft.Json;

namespace AppSage.Core.Metric
{
    public static class TypeExtensions
    {
        public static string GetSimpleGenericTypeName(this Type type)
        {
            if (type.IsGenericType)
            {
                var baseName = type.Name.Substring(0, type.Name.IndexOf('`'));
                var genericArgs = type.GetGenericArguments();
                var genericArgsString = string.Join(", ", genericArgs.Select(t => t.GetSimpleGenericTypeName()));
                return $"{baseName}<{genericArgsString}>";
            }
            else
            {
                return type.Name;
            }
        }
    }
    public class MetricValue<T> : IMetricValue<T>, IValidateMetric
    {
        public string Name { get; set; }

        public string Provider { get; set; } = string.Empty;

        public string Segment { get; set; } = string.Empty;

        public string Resource { get; set; }

        public IDictionary<string, string> Dimensions { get; set; } = new Dictionary<string,string>();

        public T? Value { get; set; }

        public bool IsLargeMetric {get;set;}

        [JsonConstructor]
        public MetricValue(string name) : this(name,default(T?))
        {
        }
        public MetricValue(string name,T? value) 
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }



 


        public List<string> Validate()
        {
            if (typeof(IValidateMetric).IsAssignableFrom(typeof(T)))
            {
                var metric = (IValidateMetric)Value;
                return metric.Validate();
            }
            else
            {
                return new List<string>();
            }
        }

        public override string ToString()
        {
            string typeName = GetType().GetSimpleGenericTypeName();
            return $"{typeName}:{this.Name}";
        }
    }
}
