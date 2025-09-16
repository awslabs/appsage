import * as vscode from 'vscode';
import { BaseViewer } from '../../shared/components/baseViewer';
import { GraphPropertyPanel } from './components/graphPropertyPanel';
import { AppSageGraph } from './types/graph';
import { AppSageLogger } from '../../shared/logging';
import { TemplateLoader } from '../../shared/utils/templateLoader';

export class GraphViewer extends BaseViewer {
    private propertyPanel = GraphPropertyPanel.getInstance();
    protected logger = AppSageLogger.getInstance();
    protected componentLogger = this.logger.forComponent('GraphViewer');

    public async resolveCustomTextEditor(
        document: vscode.TextDocument,
        webviewPanel: vscode.WebviewPanel,
        token: vscode.CancellationToken
    ): Promise<void> {
        this.componentLogger.info(`Resolving custom text editor for: ${document.uri.toString()}`);
        
        // Call parent implementation
        await super.resolveCustomTextEditor(document, webviewPanel, token);

        // Clean up graph data when panel is disposed
        webviewPanel.onDidDispose(() => {
            this.componentLogger.info(`Disposing graph viewer for: ${document.uri.toString()}`);
            this.propertyPanel.removeGraphData(document.uri.toString());
        });
    }

    protected async handleCustomMessage(message: any, document: vscode.TextDocument, webviewPanel: vscode.WebviewPanel): Promise<void> {
        const documentUri = document.uri.toString();
        this.componentLogger.debug(`Received message: ${message.type}`, message);
        
        try {
            switch (message.type) {
                case 'update':
                    this.componentLogger.info('Processing update message');
                    // Parse and store graph data when content is updated
                    try {
                        const graphData: AppSageGraph = JSON.parse(message.content);
                        this.componentLogger.info(`Parsed graph data: ${graphData.Nodes.length} nodes, ${graphData.Edges.length} edges`);
                        this.propertyPanel.setGraphData(documentUri, graphData);
                    } catch (error) {
                        this.componentLogger.error('Failed to parse graph data', error);
                    }
                    break;
                case 'nodeSelected':
                    this.componentLogger.info(`Node selected: ${message.nodeId}`);
                    if (message.nodeId) {
                        // Get node data from the property panel
                        const nodeData = this.propertyPanel.getNodeData(documentUri, message.nodeId);
                        if (nodeData) {
                            // Send node data to the webview to display in integrated panel
                            webviewPanel.webview.postMessage({
                                type: 'nodeData',
                                nodeData: nodeData
                            });
                        }
                    } else {
                        this.componentLogger.warning('nodeSelected message missing nodeId');
                    }
                    break;
                case 'edgeSelected':
                    this.componentLogger.info(`Edge selected: ${message.edgeId}`);
                    if (message.edgeId) {
                        // Get edge data from the property panel
                        const edgeData = this.propertyPanel.getEdgeData(documentUri, message.edgeId);
                        if (edgeData) {
                            // Send edge data to the webview to display in integrated panel
                            webviewPanel.webview.postMessage({
                                type: 'edgeData',
                                edgeData: edgeData
                            });
                        }
                    } else {
                        this.componentLogger.warning('edgeSelected message missing edgeId');
                    }
                    break;
                case 'selectionCleared':
                    this.componentLogger.info('Selection cleared');
                    // Send message to clear the property display
                    webviewPanel.webview.postMessage({
                        type: 'clearSelection'
                    });
                    break;
                case 'showProperties':
                    this.componentLogger.info('Show properties requested via View dropdown');
                    // Properties panel will be handled by the webview now
                    break;
                case 'hideProperties':
                    this.componentLogger.info('Hide properties requested via View dropdown');
                    // Properties panel can be closed manually by user
                    break;
                case 'showLegend':
                    this.componentLogger.info('Show legend requested via View dropdown');
                    // TODO: Implement legend functionality
                    vscode.window.showInformationMessage('Legend functionality coming soon!');
                    break;
                case 'hideLegend':
                    this.componentLogger.info('Hide legend requested via View dropdown');
                    // TODO: Implement legend hide functionality
                    break;
                case 'saveCustomization':
                    this.componentLogger.info('Saving enhanced view customization');
                    try {
                        // Save customization data to global state
                        await this.context.globalState.update('appsage.graph.customization', message.data);
                        this.componentLogger.info('Customization saved successfully');
                    } catch (error) {
                        this.componentLogger.error('Failed to save customization', error);
                        vscode.window.showErrorMessage('Failed to save graph customization');
                    }
                    break;
                case 'loadCustomization':
                    this.componentLogger.info('Loading enhanced view customization');
                    try {
                        // Load customization data from global state
                        const customizationData = this.context.globalState.get('appsage.graph.customization');
                        webviewPanel.webview.postMessage({
                            type: 'customizationLoaded',
                            data: customizationData
                        });
                        this.componentLogger.info('Customization loaded successfully');
                    } catch (error) {
                        this.componentLogger.error('Failed to load customization', error);
                    }
                    break;
                default:
                    this.componentLogger.debug(`Unknown message type: ${message.type}`);
                    break;
            }
        } catch (error) {
            this.componentLogger.error('Error handling custom message', error);
        }
    }
    
    protected getHtmlForWebview(webview: vscode.Webview, document: vscode.TextDocument): string {
        const styleUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'graph.css')
        );
        const scriptUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'graph.js')
        );

        // Use local Cytoscape files instead of CDN
        const cytoscapeUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'lib', 'cytoscape.min.js')
        );
        const cytoscapeCoseBilkentUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'lib', 'cytoscape-cose-bilkent.js')
        );

        // Enhanced view customization scripts
        const graphCustomizationUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'graph-customization.js')
        );
        const enhancedViewCustomizerUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'enhanced-view-customizer.js')
        );

        // Refactored component scripts
        const graphRendererUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'GraphRenderer.js')
        );
        const topMenuUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'TopMenu.js')
        );
        const sidePanelUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'graph', 'SidePanel.js')
        );

        const nonce = TemplateLoader.generateNonce();
        const csp = TemplateLoader.createCSP(webview, nonce);

        // Load the HTML template and replace placeholders
        return TemplateLoader.loadTemplate(this.context, 'graph/graph.html', webview, {
            CSP_SOURCE: csp,
            NONCE: nonce,
            STYLE_URI: styleUri.toString(),
            SCRIPT_URI: scriptUri.toString(),
            CYTOSCAPE_URI: cytoscapeUri.toString(),
            CYTOSCAPE_COSE_BILKENT_URI: cytoscapeCoseBilkentUri.toString(),
            GRAPH_CUSTOMIZATION_URI: graphCustomizationUri.toString(),
            ENHANCED_VIEW_CUSTOMIZER_URI: enhancedViewCustomizerUri.toString(),
            GRAPH_RENDERER_URI: graphRendererUri.toString(),
            TOP_MENU_URI: topMenuUri.toString(),
            SIDE_PANEL_URI: sidePanelUri.toString()
        });
    }
}
