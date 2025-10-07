import * as vscode from 'vscode';
import { AppSageLogger } from '../../../shared/logging';
import { TemplateLoader } from '../../../shared/utils/templateLoader';

/**
 * Manages property data for the integrated chart property panel
 */
export class ChartPropertyPanel {
    private static _instance: ChartPropertyPanel;
    private _context: vscode.ExtensionContext | undefined;
    private _logger: AppSageLogger | undefined;
    private _componentLogger: any;
    private _currentProperties: any = null;

    private constructor() {
        // Private constructor for singleton
    }

    public static getInstance(): ChartPropertyPanel {
        if (!ChartPropertyPanel._instance) {
            ChartPropertyPanel._instance = new ChartPropertyPanel();
        }
        return ChartPropertyPanel._instance;
    }

    public setLogger(logger: AppSageLogger): void {
        this._logger = logger;
        this._componentLogger = logger.forComponent('ChartPropertyPanel');
        this._componentLogger.info('Chart Property Panel logger initialized');
    }

    public setContext(context: vscode.ExtensionContext): void {
        this._context = context;
        this._componentLogger?.info('Chart Property Panel context set');
    }

    public updateProperties(properties: any): void {
        this._currentProperties = properties;
        this._componentLogger?.debug('Chart properties updated', { properties });
    }

    public getCurrentProperties(): any {
        return this._currentProperties;
    }

    public clearProperties(): void {
        this._currentProperties = null;
        this._componentLogger?.debug('Chart properties cleared');
    }

    public getWebviewContent(webview: vscode.Webview): string {
        if (!this._context) {
            throw new Error('Context not set. Call setContext() first.');
        }

        const styleUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this._context.extensionUri, 'webview', 'chart', 'properties.css')
        );

        const sharedUtilsUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this._context.extensionUri, 'webview', 'shared', 'js', 'shared-utils.js')
        );

        const domPurifyUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this._context.extensionUri, 'webview', 'lib', 'dompurify.min.js')
        );

        return TemplateLoader.loadTemplate(this._context, 'chart/properties.html', webview, {
            STYLE_URI: styleUri.toString(),
            SHARED_UTILS_URI: sharedUtilsUri.toString(),
            DOMPURIFY_URI: domPurifyUri.toString()
        });
    }
}
