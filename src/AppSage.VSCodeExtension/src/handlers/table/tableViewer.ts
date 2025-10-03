import * as vscode from 'vscode';
import { BaseViewer } from '../../shared/components/baseViewer';
import { TemplateLoader } from '../../shared/utils/templateLoader';

export class TableViewer extends BaseViewer {
    
    protected getHtmlForWebview(webview: vscode.Webview, document: vscode.TextDocument): string {
        // Local asset URIs - only CSS and JS needed for pure HTML table
        const styleUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'table', 'table.css')
        );
        const scriptUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'table', 'table.js')
        );

        const nonce = TemplateLoader.generateNonce();
        const csp = TemplateLoader.createCSP(webview, nonce);

        // Load the HTML template and replace placeholders
        return TemplateLoader.loadTemplate(this.context, 'table/table.html', webview, {
            CSP_SOURCE: csp,
            NONCE: nonce,
            STYLE_URI: styleUri.toString(),
            SCRIPT_URI: scriptUri.toString()
        });
    }
}
