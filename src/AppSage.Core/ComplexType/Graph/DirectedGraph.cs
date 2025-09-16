using Newtonsoft.Json;

namespace AppSage.Core.ComplexType.Graph
{
    public class DirectedGraph: IDirectedGraph
    {
        private object _padlock = new object();

        //ensure serialization of private property
        [JsonProperty("Nodes")]
        private HashSet<INode> _nodes = new();
        //ensure serialization of private property
        [JsonProperty("Edges")]
        private HashSet<IEdge> _edges = new();

        [JsonIgnore]
        public IReadOnlyList<INode> Nodes => _nodes.ToList();
        [JsonIgnore]
        public IReadOnlyList<IEdge> Edges => _edges.ToList();

        public bool ContainsNode(INode node)
        {
            lock (_padlock)
            {
                return _nodes.Contains(node);
            }
        }
        public bool ContainsEdge(IEdge edge)
        {
            lock (_padlock)
            {
                return _edges.Contains(edge);
            }
        }

        public INode GetNode(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            lock (_padlock)
            {
                return _nodes.FirstOrDefault(n => n.Id == id);
            }

        }
        public IEdge GetEdge(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            lock (_padlock)
            {
                return _edges.FirstOrDefault(e => e.Id==id);
            }

        }


        public INode AddOrUpdateNode(string id)
        {
            return AddOrUpdateNode(id, string.Empty,string.Empty,new Dictionary<string,string>());
        }
        public INode AddOrUpdateNode(string id, string name)
        {
            return AddOrUpdateNode(id, name, string.Empty, new Dictionary<string, string>());
        }
        public INode AddOrUpdateNode(string id, string name,string type)
        {
            return AddOrUpdateNode(id, name, type, new Dictionary<string, string>());
        }
        public INode AddOrUpdateNode(string id, string name,string type,IReadOnlyDictionary<string,string> attributes)
        {
            INode node = null;
            lock (_padlock)
            {
                if (_nodes.Any(n => n.Id == id))
                {
                    node = _nodes.First(n => n.Id == id);
                    node.Name = name; // Update name if node already exists

                    //For each attribute in the attributes dictionary, add or update it in the node
                    node.AddOrUpdateAttribute(attributes);

                }
                else
                {
                    node = new Node(id, name,type,attributes);
                    _nodes.Add(node);
                }
            }
            return node;
        }
        public INode AddOrUpdateNode(INode node)
        {
            lock (_padlock)
            {
                if (_nodes.Any(n => n.Id == node.Id))
                {
                    var existingNode = _nodes.First(n => n.Id == node.Id);
                    existingNode.Name = node.Name; // Update name if node already exists

                    //Update all attributes
                    existingNode.AddOrUpdateAttribute(node.Attributes);

                    return existingNode;


                }
                else
                {
                    _nodes.Add(node);
                }
            }
            return node;
        }

        public IEdge AddOrUpdateEdge(string id, INode source, INode target) { 
            return AddOrUpdateEdge(id, string.Empty, string.Empty, new Dictionary<string, string>(), source, target);
        }
        public IEdge AddOrUpdateEdge(INode source, INode target, string type)
        {
            string edgeId = GraphUtility.GetEdgeId(source, target, type);
            string edgeName = GraphUtility.GetEdgeName(source, target, type);
            return AddOrUpdateEdge(edgeId, edgeName, type, source, target);
        }
        
        public IEdge AddOrUpdateEdge(string id,string name, INode source, INode target)
        {
            return AddOrUpdateEdge(id, name, string.Empty, new Dictionary<string, string>(), source, target);
        }
        public IEdge AddOrUpdateEdge(string id, string name,string type, INode source, INode target)
        {
            return AddOrUpdateEdge(id, name, type, new Dictionary<string, string>(), source, target);
        }
        public IEdge AddOrUpdateEdge(string id,string name,string type,IReadOnlyDictionary<string,string> attributes,INode source, INode target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            lock (_padlock)
            {
                // Ensure nodes are in the graph
                if (!this.ContainsNode(source)) { AddOrUpdateNode(source); }
                if (!this.ContainsNode(target)) { AddOrUpdateNode(target); }
                
                // Ensure no duplicate edges
                if (_edges.Any(e => e.Id == id))
                {
                    // Update existing edge
                    var existingEdge = _edges.First(e => e.Id == id);
                    existingEdge.Source = source;
                    existingEdge.Target = target;
                    existingEdge.Name = name; // Update name if edge already exists
                    existingEdge.AddOrUpdateAttribute(attributes);
                    return existingEdge;
                }
                else
                {
                    var edge = new Edge(id, name,type, source, target,attributes);
                    _edges.Add(edge);
                    return edge;
                }
            }
        }
        public IEdge AddOrUpdateEdge(IEdge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            if (edge.Source == null) throw new ArgumentNullException(nameof(edge.Source));
            if (edge.Target == null) throw new ArgumentNullException(nameof(edge.Target));
            lock (_padlock)
            {
                
                // Ensure nodes are in the graph
                if (!this.ContainsNode(edge.Source)) { AddOrUpdateNode(edge.Source); }
                if (!this.ContainsNode(edge.Target)) { AddOrUpdateNode(edge.Target); }
                
                // Ensure no duplicate edges
                if (_edges.Any(e => e.Id == edge.Id))
                {
                    // Update existing edge
                    var existingEdge = _edges.First(e => e.Id == edge.Id);
                    existingEdge.Source = edge.Source;
                    existingEdge.Target = edge.Target;
                    existingEdge.Name = edge.Name; // Update name if edge already exists
                    existingEdge.AddOrUpdateAttribute(edge.Attributes);
                    return existingEdge;
                }
                else
                {
                    _edges.Add(edge);
                    return edge;
                }
            }
        }
        
        public bool RemoveNode(INode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            bool status = false;
            lock (_padlock)
            {
                // First remove all edges connected to this node
                var edgesToRemove = _edges.Where(e => e.Source.Equals(node) || e.Target.Equals(node)).ToList();
                foreach (var edge in edgesToRemove)
                {
                    _edges.Remove(edge);
                }
                status = _nodes.Remove(node);
            }
            return status;
        }
        public bool RemoveEdge(IEdge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            lock (_padlock)
            {
                return _edges.Remove(edge);
            }
        }

        public static DirectedGraph MergeGraph(IEnumerable<DirectedGraph> graphs)
        {
            if (graphs == null) throw new ArgumentNullException(nameof(graphs));


            DirectedGraph mergedGraph = new DirectedGraph();

            foreach (var graph in graphs)
            {
                if (graph == null) continue;
                // Add nodes
                foreach (var node in graph.Nodes)
                {
                    mergedGraph.AddOrUpdateNode(node);
                }
                // Add edges
                foreach (var edge in graph.Edges)
                {
                    mergedGraph.AddOrUpdateEdge(edge);
                }
            }
            return mergedGraph;
        }

        /// <summary>
        /// Validate the graph for common issues such as duplicate nodes, duplicate edges, and edges that reference non-existent nodes.
        /// This is useful for ensuring the integrity of the graph before performing operations on it. 
        /// Specially after deserialization or before saving to a database.
        /// </summary>
        /// <returns>List of errors if exist</returns>
        public IEnumerable<string> Validate()
        {
            var errors = new List<string>();
            lock (_padlock)
            {
                // Check for duplicate nodes
                var duplicateNodes = _nodes.GroupBy(n => n.Id).Where(g => g.Count() > 1);
                foreach (var group in duplicateNodes)
                {
                    errors.Add($"Duplicate node found with ID: {group.Key}. Each node must have a unique id");
                }
                // Check for duplicate edges
                var duplicateEdges = _edges.GroupBy(e => e.Id).Where(g => g.Count() > 1);
                foreach (var group in duplicateEdges)
                {
                    errors.Add($"Duplicate edge found with ID: {group.Key}. Each edge must have a unique id");
                }
                // Check for edges with non-existent nodes
                foreach (var edge in _edges)
                {
                    if (!_nodes.Contains(edge.Source))
                    {
                        errors.Add($"Edge {edge.Id} references a non-existent source node {edge.Source.Id}");
                    }
                    if (!_nodes.Contains(edge.Target))
                    {
                        errors.Add($"Edge {edge.Id} references a non-existent target node {edge.Target.Id}");
                    }
                }
                return errors;
            }
        }
        
        
        /// <summary>
        /// Link the given source node to all nodes that match the target selection criteria.
        /// </summary>
        /// <param name="source">Source node</param>
        /// <param name="targetSelectionCriteria">How the existing nodes are selected for create a link</param>
        /// <param name="edgeDefinition">How the edge between source and the targets are created</param>
        /// <returns>List of edges created or updated</returns>
        private IEnumerable<IEdge> AddOrUpdateEdgeWithTargetSelection(INode source, NodeSelectionCriteria targetSelectionCriteria,EdgeDefinition edgeDefinition)
        {
            lock (_padlock)
            {
                var targets = _nodes.Where(target => targetSelectionCriteria(source, target)).Select(n => n);
                foreach(var target in targets)
                {
                    var (edgeId,edgeName, edgeType, edgeAttributes) = edgeDefinition(source, target);
                    yield return AddOrUpdateEdge(edgeId, edgeName, edgeType, edgeAttributes, source, target);
                }
            }
        }

        /// <summary>
        /// Link the given target node to all nodes that match the source selection criteria.
        /// </summary>
        /// <param name="target">Target to be linked</param>
        /// <param name="sourceSelectionCriteria">How the existing sources are selected to create the link</param>
        /// <param name="edgeDefinition">How the edge between source and target is created</param>
        /// <returns>List of edges created or updated</returns>
        private IEnumerable<IEdge> AddOrUpdateEdgeWithSourceSelection(INode target, NodeSelectionCriteria sourceSelectionCriteria, EdgeDefinition edgeDefinition)
        {
            lock (_padlock)
            {
                var sources = _nodes.Where(source => sourceSelectionCriteria(source, target)).Select(n => n);
                foreach (var source in sources)
                {
                    var (edgeId, edgeName, edgeType, edgeAttributes) = edgeDefinition(source, target);
                    yield return AddOrUpdateEdge(edgeId, edgeName, edgeType, edgeAttributes, source, target);
                }
            }
        }

        /// <summary>
        /// Get all nodes (targets) that are adjacent to a given node (source). 
        /// return the nodes reached by outgoing edges from node.
        //  In dependency terms: this returns things the given node depends on(its providers / dependencies).
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<INode> GetAdjacentNodes(INode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            lock (_padlock)
            {
                return _edges
              .Where(e => e.Source.Equals(node))
              .Select(e => e.Target)
              .Distinct();
            }
        }

        /// <summary>
        /// Get all predecessors of a node. Given a node (target), this method returns all nodes (sources) that have an incoming edge pointing to it (target).
        /// return the nodes that have incoming edges to node.
        //  In dependency terms: the things that depend on the given node(its consumers / dependents).
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<INode> GetPredecessors(INode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            lock (_padlock)
            {
                return _edges
                .Where(e => e.Target.Equals(node))
                .Select(e => e.Source)
                .Distinct();
            }
        }

        public IEnumerable<IEdge> GetOutgoingEdges(INode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            lock (_padlock)
            {
                return _edges.Where(e => e.Source.Equals(node));
            }
        }

        public IEnumerable<IEdge> GetIncomingEdges(INode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            lock (_padlock)
            {
                return _edges.Where(e => e.Target.Equals(node));
            }
        }

        public bool HasPath(INode source, INode target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var visited = new HashSet<INode>();
            var queue = new Queue<INode>();

            lock (_padlock)
            {
                queue.Enqueue(source);
                visited.Add(source);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();

                    if (current.Equals(target))
                        return true;

                    foreach (var adjacent in GetAdjacentNodes(current))
                    {
                        if (!visited.Contains(adjacent))
                        {
                            visited.Add(adjacent);
                            queue.Enqueue(adjacent);
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<INode> BreadthFirstTraversal(INode startNode)
        {
            if (startNode == null) throw new ArgumentNullException(nameof(startNode));
            if (!_nodes.Contains(startNode)) throw new ArgumentException("Start node is not in the graph");

            var visited = new HashSet<INode>();
            var queue = new Queue<INode>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                yield return current;

                foreach (var adjacent in GetAdjacentNodes(current))
                {
                    if (!visited.Contains(adjacent))
                    {
                        visited.Add(adjacent);
                        queue.Enqueue(adjacent);
                    }
                }
            }
        }

        public IEnumerable<INode> DepthFirstTraversal(INode startNode)
        {
            if (startNode == null) throw new ArgumentNullException(nameof(startNode));
            if (!_nodes.Contains(startNode)) throw new ArgumentException("Start node is not in the graph");

            var visited = new HashSet<INode>();
            return DepthFirstTraversalImpl(startNode, visited);
        }

        private IEnumerable<INode> DepthFirstTraversalImpl(INode node, HashSet<INode> visited)
        {
            visited.Add(node);
            yield return node;

            foreach (var adjacent in GetAdjacentNodes(node))
            {
                if (!visited.Contains(adjacent))
                {
                    foreach (var descendant in DepthFirstTraversalImpl(adjacent, visited))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        public IReadOnlyList<INode> GetTopologicalSort()
        {
            var visited = new HashSet<INode>();
            var tempMarked = new HashSet<INode>();
            var result = new List<INode>();
            lock (_padlock)
            {
                foreach (var node in _nodes)
                {
                    if (!visited.Contains(node))
                    {
                        TopologicalSortVisit(node, visited, tempMarked, result);
                    }
                }

                result.Reverse();
            }
            return result;
        }

        private void TopologicalSortVisit(INode node, HashSet<INode> visited, HashSet<INode> tempMarked, List<INode> result)
        {
            if (tempMarked.Contains(node))
                throw new InvalidOperationException("Graph contains a cycle, cannot perform topological sort");

            if (!visited.Contains(node))
            {
                tempMarked.Add(node);

                foreach (var adjacent in GetAdjacentNodes(node))
                {
                    TopologicalSortVisit(adjacent, visited, tempMarked, result);
                }

                tempMarked.Remove(node);
                visited.Add(node);
                result.Add(node);
            }
        }

        public IEnumerable<IEnumerable<INode>> FindAllPaths(INode source, INode target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var path = new List<INode>();
            var paths = new List<List<INode>>();
            var visited = new HashSet<INode>();

            FindAllPathsDFS(source, target, visited, path, paths);

            return paths;
        }

        private void FindAllPathsDFS(INode current, INode target, HashSet<INode> visited,
            List<INode> path, List<List<INode>> paths)
        {
            visited.Add(current);
            path.Add(current);

            if (current.Equals(target))
            {
                paths.Add(new List<INode>(path));
            }
            else
            {
                foreach (var adjacent in GetAdjacentNodes(current))
                {
                    if (!visited.Contains(adjacent))
                    {
                        FindAllPathsDFS(adjacent, target, visited, path, paths);
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            visited.Remove(current);
        }
    }
}
