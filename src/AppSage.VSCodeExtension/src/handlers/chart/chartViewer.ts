import * as vscode from 'vscode';
import { BaseViewer } from '../../shared/components/baseViewer';
import { ChartPropertyPanel } from './components/chartPropertyPanel';
import { ChartContentProvider } from './providers/chartContentProvider';
import { AppSageLogger } from '../../shared/logging';
import { TemplateLoader } from '../../shared/utils/templateLoader';

export class ChartViewer extends BaseViewer {
    private propertyPanel = ChartPropertyPanel.getInstance();
    private contentProvider: ChartContentProvider;
    protected logger = AppSageLogger.getInstance();
    protected componentLogger = this.logger.forComponent('ChartViewer');

    constructor(context: vscode.ExtensionContext) {
        super(context);
        this.contentProvider = new ChartContentProvider(this.logger);
        this.propertyPanel.setContext(context);
        this.propertyPanel.setLogger(this.logger);
        this.componentLogger.info('Chart viewer initialized');
    }

    public async resolveCustomTextEditor(
        document: vscode.TextDocument,
        webviewPanel: vscode.WebviewPanel,
        token: vscode.CancellationToken
    ): Promise<void> {
        this.componentLogger.info('Resolving chart editor', { 
            fileName: document.fileName,
            fileSize: document.getText().length 
        });

        // Validate the document content
        const content = document.getText();
        if (!this.contentProvider.validateChartData(content)) {
            this.componentLogger.error('Invalid chart data format');
            vscode.window.showErrorMessage('Invalid chart data format. Please check the file content.');
            return;
        }

        await super.resolveCustomTextEditor(document, webviewPanel, token);
    }

    protected async handleCustomMessage(message: any, document: vscode.TextDocument, webviewPanel: vscode.WebviewPanel): Promise<void> {
        this.componentLogger.debug('Handling message', { command: message.command });

        switch (message.command) {
            case 'chartReady':
                this.handleChartReady(document, webviewPanel);
                break;
            case 'updateChart':
                this.handleUpdateChart(message, document, webviewPanel);
                break;
            case 'exportChart':
                this.handleExportChart(message);
                break;
            case 'getTableSummary':
                this.handleGetTableSummary(document, webviewPanel);
                break;
            case 'error':
                this.componentLogger.error('Chart error', { error: message.error });
                vscode.window.showErrorMessage(`Chart Error: ${message.error}`);
                break;
            default:
                this.componentLogger.warning('Unhandled message command', { command: message.command });
                break;
        }
    }

    private handleChartReady(document: vscode.TextDocument, webviewPanel: vscode.WebviewPanel): void {
        try {
            const content = document.getText();
            const chartData = this.contentProvider.parseChartData(content);
            const tableSummary = this.contentProvider.getTableSummary(content);

            webviewPanel.webview.postMessage({
                command: 'updateChartData',
                data: chartData,
                summary: tableSummary
            });

            this.componentLogger.info('Chart data sent to webview', { 
                tableCount: chartData.tables.length 
            });
        } catch (error) {
            this.componentLogger.error('Failed to handle chart ready', { error });
            webviewPanel.webview.postMessage({
                command: 'error',
                error: 'Failed to load chart data'
            });
        }
    }

    private handleUpdateChart(message: any, document: vscode.TextDocument, webviewPanel: vscode.WebviewPanel): void {
        try {
            this.componentLogger.info('Chart configuration updated', { 
                chartType: message.chartType,
                selectedTables: message.selectedTables 
            });

            // Store current chart configuration for property panel
            this.propertyPanel.updateProperties({
                chartType: message.chartType,
                selectedTables: message.selectedTables,
                customizations: message.customizations
            });
        } catch (error) {
            this.componentLogger.error('Failed to handle chart update', { error });
        }
    }

    private handleExportChart(message: any): void {
        try {
            // Future implementation for chart export
            this.componentLogger.info('Chart export requested', { format: message.format });
            vscode.window.showInformationMessage('Chart export feature coming soon!');
        } catch (error) {
            this.componentLogger.error('Failed to handle chart export', { error });
        }
    }

    private handleGetTableSummary(document: vscode.TextDocument, webviewPanel: vscode.WebviewPanel): void {
        try {
            const content = document.getText();
            const summary = this.contentProvider.getTableSummary(content);

            webviewPanel.webview.postMessage({
                command: 'tableSummary',
                summary: summary
            });

            this.componentLogger.info('Table summary sent', { tableCount: summary.length });
        } catch (error) {
            this.componentLogger.error('Failed to get table summary', { error });
        }
    }

    protected getHtmlForWebview(webview: vscode.Webview, document: vscode.TextDocument): string {
        const styleUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'chart', 'chart.css')
        );
        const scriptUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'chart', 'chart.js')
        );

        // Use local ECharts files
        const echartsUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'lib', 'echarts.min.js')
        );

        // Shared utilities
        const sharedUtilsUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'shared', 'js', 'shared-utils.js')
        );
        
        // WebView logger for secure logging
        const webviewLoggerUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'shared', 'js', 'webview-logger.js')
        );

        // DOMPurify for XSS prevention
        const domPurifyUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'lib', 'dompurify.min.js')
        );

        // Chart component scripts
        const chartRendererUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'chart', 'ChartRenderer.js')
        );
        const chartControlsUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'chart', 'ChartControls.js')
        );

        const nonce = TemplateLoader.generateNonce();
        const csp = TemplateLoader.createCSP(webview, nonce);

        // Load the HTML template and replace placeholders
        return TemplateLoader.loadTemplate(this.context, 'chart/chart.html', webview, {
            CSP_SOURCE: csp,
            NONCE: nonce,
            STYLE_URI: styleUri.toString(),
            SCRIPT_URI: scriptUri.toString(),
            ECHARTS_URI: echartsUri.toString(),
            CHART_RENDERER_URI: chartRendererUri.toString(),
            CHART_CONTROLS_URI: chartControlsUri.toString(),
            DOMPURIFY_URI: domPurifyUri.toString(),
            SHARED_UTILS_URI: sharedUtilsUri.toString(),
            WEBVIEW_LOGGER_URI: webviewLoggerUri.toString()
        });
    }
}
