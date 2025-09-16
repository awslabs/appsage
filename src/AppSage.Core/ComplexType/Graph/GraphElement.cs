using Newtonsoft.Json;

namespace AppSage.Core.ComplexType.Graph
{
    public abstract class GraphElement: IGraphElement
    {
        private const string _UNDEFINED= "Undefined";
        /// <summary>
        /// Unique identifier for the graph element. For a given graph , this should be unique.
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }
        /// <summary>
        /// Name of the graph element. This is not required to be unique.
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Classifiction of the graph elment. This can be used to differentiate between different types of elements. 
        /// For example a Node can be of type class, function, enum, person, country, virtual machine, etc. A node is usually a noun.
        /// For example an edge can be of type "calls", "inherits", "depends on", "is part of", "owns", "resides". An edge is usually a verb.
        /// </summary>
        [JsonProperty("Type")]
        public string Type { get; set; }


        // Default constructor for deserialization
        [JsonConstructor]
        protected GraphElement(){
            Id = string.Empty;
            Name = string.Empty;
            Type = _UNDEFINED;
        }

        [JsonProperty("Attributes")]
        private Dictionary<string, string> _attributes { get; set; }=new Dictionary<string, string>();

        /// <summary>
        /// Any key value pairs you want to keep along with this element.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> Attributes => _attributes;


       
        public GraphElement(string id) : this(id, string.Empty,string.Empty, new Dictionary<string,string>()) { }
        public GraphElement(string id, string name) : this(id, name,string.Empty, new Dictionary<string, string>()) { }
        public GraphElement(string id, string name, string type) : this(id, name, type, new Dictionary<string, string>()) { }
        public GraphElement(string id, string name,string type, IReadOnlyDictionary<string, string> attributes)
        {
            if(string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "Id cannot be null or empty.");
            }
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = string.IsNullOrEmpty(type) ? _UNDEFINED : type;

            if (attributes is null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            foreach(var kvp in attributes)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    throw new ArgumentNullException(nameof(kvp.Key), "Key cannot be null or empty.");
                }
            }

            foreach(var kvp in attributes)
            {
                _attributes.Add(kvp.Key, kvp.Value);
            }

        }

        public void AddOrUpdateAttribute(string key, string value) {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "Key cannot be null or empty.");
            }
            lock (_attributes)
            {
                if (_attributes.ContainsKey(key))
                {
                    _attributes[key] = value;
                }
                else
                {
                    _attributes.Add(key, value);
                }
            }
        }

        public void AddOrUpdateAttribute(IReadOnlyDictionary<string,string> attributes)
        {
            if(attributes is null)
            {
                throw new ArgumentNullException(nameof(attributes), "Attributes cannot be null or empty.");
            }
            lock (_attributes)
            {
                foreach(var kvp in attributes)
                {
                    if (string.IsNullOrEmpty(kvp.Key))
                    {
                        throw new ArgumentNullException(nameof(kvp.Key), "Key cannot be null or empty.");
                    }
                    if (_attributes.ContainsKey(kvp.Key))
                    {
                        _attributes[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        _attributes.Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            else if (obj is GraphElement ge)
            {
                if ((obj is Node && this is Edge) || (obj is Edge && this is Node))
                {
                    return false; // Node and Edge are not equal
                }
                else if (ge.Id == Id)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
