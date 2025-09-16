using Newtonsoft.Json;

namespace AppSage.Core.ComplexType.Graph
{
    internal class Edge : GraphElement,IEdge
    {
        [JsonProperty("Source")]
        public INode Source { get; set; }

        [JsonProperty("Target")]
        public INode Target { get; set; }

        // Default constructor for deserialization
        [JsonConstructor]
        protected Edge() : base() { }

        internal Edge(string id, INode source, INode target) : this(id, string.Empty,string.Empty, source, target,new Dictionary<string,string>()) { }
        internal Edge(string id,string name, INode source, INode target) : this(id,name, string.Empty, source, target, new Dictionary<string, string>()) { }
        internal Edge(string id, string name,string type, INode source, INode target) : this(id, name, type, source, target, new Dictionary<string, string>()) { }

        internal Edge(string id, string name,string type, INode source, INode target, IReadOnlyDictionary<string, string> attributes)
            : base(id, name,type, attributes)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return $"{Type}:{Source.Name}>[]>{Target.Name}";
            }
            else
            {
                return $"{Type}:{Name}";
            }
        }
    }
}
