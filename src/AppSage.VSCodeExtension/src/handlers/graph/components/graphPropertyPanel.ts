import * as vscode from 'vscode';
import { GraphNode, GraphEdge, AppSageGraph } from '../types/graph';
import { AppSageLogger } from '../../../shared/logging';

export interface GraphPropertyData {
    id: string;
    type: 'node' | 'edge';
    name?: string;
    attributes?: { [key: string]: any };
}

/**
 * Manages property data for the integrated graph property panel
 */
export class GraphPropertyPanel {
    private static instance: GraphPropertyPanel;
    private graphDataMap = new Map<string, AppSageGraph>();
    private logger?: AppSageLogger;
    private componentLogger: any;

    private constructor() {}

    public static getInstance(): GraphPropertyPanel {
        if (!GraphPropertyPanel.instance) {
            GraphPropertyPanel.instance = new GraphPropertyPanel();
        }
        return GraphPropertyPanel.instance;
    }

    public setLogger(logger: AppSageLogger) {
        this.logger = logger;
        this.componentLogger = logger.forComponent('GraphPropertyPanel');
        this.componentLogger.info('Logger initialized');
    }

    private log(message: string, ...args: any[]) {
        if (this.componentLogger) {
            this.componentLogger.info(message, ...args);
        } else {
            console.log(`[GraphPropertyPanel] ${message}`, ...args);
        }
    }

    private logError(message: string, ...args: any[]) {
        if (this.componentLogger) {
            this.componentLogger.error(message, ...args);
        } else {
            console.error(`[GraphPropertyPanel] ${message}`, ...args);
        }
    }

    private logWarning(message: string, ...args: any[]) {
        if (this.componentLogger) {
            this.componentLogger.warning(message, ...args);
        } else {
            console.warn(`[GraphPropertyPanel] ${message}`, ...args);
        }
    }

    public setGraphData(documentUri: string, graphData: AppSageGraph) {
        this.graphDataMap.set(documentUri, graphData);
        this.log(`Graph data set for ${documentUri}: ${graphData.Nodes.length} nodes, ${graphData.Edges.length} edges`);
    }

    public removeGraphData(documentUri: string) {
        this.graphDataMap.delete(documentUri);
        this.log(`Graph data removed for ${documentUri}`);
    }

    public getNodeData(documentUri: string, nodeId: string): GraphPropertyData | null {
        this.log(`Getting node data: ${nodeId} in document: ${documentUri}`);
        
        const graphData = this.graphDataMap.get(documentUri);
        if (!graphData) {
            this.logError(`No graph data found for document: ${documentUri}`);
            return null;
        }

        const node = graphData.Nodes.find(n => n.Id === nodeId);
        if (node) {
            this.log(`Found node: ${node.Name} (${node.Type})`);
            const propertyData: GraphPropertyData = {
                id: node.Id,
                type: 'node',
                name: node.Name,
                attributes: {
                    nodeType: node.Type,
                    ...node.Attributes
                }
            };
            return propertyData;
        } else {
            this.logWarning(`Node with ID '${nodeId}' not found in graph data`);
            return null;
        }
    }

    public getEdgeData(documentUri: string, edgeId: string): GraphPropertyData | null {
        this.log(`Getting edge data: ${edgeId} in document: ${documentUri}`);
        
        const graphData = this.graphDataMap.get(documentUri);
        if (!graphData) {
            this.logError(`No graph data found for document: ${documentUri}`);
            return null;
        }

        const edge = graphData.Edges.find(e => e.Id === edgeId);
        if (edge) {
            this.log(`Found edge: ${edge.Name} (${edge.Type})`);
            const propertyData: GraphPropertyData = {
                id: edge.Id,
                type: 'edge',
                name: edge.Name,
                attributes: {
                    edgeType: edge.Type,
                    source: edge.Source.Id,
                    target: edge.Target.Id,
                    ...edge.Attributes
                }
            };
            return propertyData;
        } else {
            this.logWarning(`Edge with ID '${edgeId}' not found in graph data`);
            return null;
        }
    }
}
