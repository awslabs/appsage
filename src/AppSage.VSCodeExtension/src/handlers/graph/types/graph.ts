export interface GraphNode {
    Id: string;
    Name: string;
    Type: string;
    Attributes: Record<string, any>;
}

export interface GraphEdge {
    Id: string;
    Name: string;
    Type: string;
    Source: GraphNode;
    Target: GraphNode;
    Attributes: Record<string, any>;
}

export interface AppSageGraph {
    Nodes: GraphNode[];
    Edges: GraphEdge[];
}
