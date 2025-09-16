using Newtonsoft.Json;

namespace AppSage.Core.ComplexType.Graph
{
    internal class Node : GraphElement, INode
    {
        // Default constructor for deserialization
        [JsonConstructor]
        protected Node() : base() { }
        internal Node(string id) : this(id,string.Empty,string.Empty,new Dictionary<string,string>()) { }
        internal Node(string id,string name) : this(id, name, string.Empty, new Dictionary<string, string>()) { }
        internal Node(string id, string name,string type) : this(id, name, type, new Dictionary<string, string>()) { }
        internal Node(string id,string name,string type,IReadOnlyDictionary<string,string> attributes) : base(id,name,type,attributes) { }

        public override string ToString()
        {
            if(string.IsNullOrEmpty(Name))
            {
                return $"{Type}:{Id}";
            }
            else
            {
                return $"{Type}:{Name}";
            }   
        }
    }
}
