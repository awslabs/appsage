import * as vscode from 'vscode';
import { GraphNode, GraphEdge, AppSageGraph } from '../types/graph';
import { AppSageLogger } from '../../../shared/logging';
import { TemplateLoader } from '../../../shared/utils/templateLoader';

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
    private componentLogger?: ReturnType<AppSageLogger['forComponent']>;
    private _context?: vscode.ExtensionContext;

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
        this.componentLogger.info('GraphPropertyPanel logger initialized');
    }

    public setContext(context: vscode.ExtensionContext) {
        this._context = context;
    }



    public setGraphData(documentUri: string, graphData: AppSageGraph) {
        this.graphDataMap.set(documentUri, graphData);
        this.componentLogger?.info(`Graph data set for ${documentUri}: ${graphData.Nodes.length} nodes, ${graphData.Edges.length} edges`);
    }

    public removeGraphData(documentUri: string) {
        this.graphDataMap.delete(documentUri);
        this.componentLogger?.info(`Graph data removed for ${documentUri}`);
    }

    public getNodeData(documentUri: string, nodeId: string): GraphPropertyData | null {
        this.componentLogger?.info(`Getting node data: ${nodeId} in document: ${documentUri}`);
        
        const graphData = this.graphDataMap.get(documentUri);
        if (!graphData) {
            this.componentLogger?.error(`No graph data found for document: ${documentUri}`);
            return null;
        }

        const node = graphData.Nodes.find(n => n.Id === nodeId);
        if (node) {
            this.componentLogger?.info(`Found node: ${node.Name} (${node.Type})`);
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
            this.componentLogger?.warning(`Node with ID '${nodeId}' not found in graph data`);
            return null;
        }
    }

    public getEdgeData(documentUri: string, edgeId: string): GraphPropertyData | null {
        this.componentLogger?.info(`Getting edge data: ${edgeId} in document: ${documentUri}`);
        
        const graphData = this.graphDataMap.get(documentUri);
        if (!graphData) {
            this.componentLogger?.error(`No graph data found for document: ${documentUri}`);
            return null;
        }

        const edge = graphData.Edges.find(e => e.Id === edgeId);
        if (edge) {
            this.componentLogger?.info(`Found edge: ${edge.Name} (${edge.Type})`);
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
            this.componentLogger?.warning(`Edge with ID '${edgeId}' not found in graph data`);
            return null;
        }
    }

    public getWebviewContent(webview: vscode.Webview): string {
        if (!this._context) {
            throw new Error('Context not set. Call setContext() first.');
        }

        const styleUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this._context.extensionUri, 'webview', 'graph', 'properties.css')
        );

        const sharedUtilsUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this._context.extensionUri, 'webview', 'shared', 'js', 'shared-utils.js')
        );

        const domPurifyUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this._context.extensionUri, 'webview', 'lib', 'dompurify.min.js')
        );

        return TemplateLoader.loadTemplate(this._context, 'graph/properties.html', webview, {
            STYLE_URI: styleUri.toString(),
            SHARED_UTILS_URI: sharedUtilsUri.toString(),
            DOMPURIFY_URI: domPurifyUri.toString()
        });
    }
}
