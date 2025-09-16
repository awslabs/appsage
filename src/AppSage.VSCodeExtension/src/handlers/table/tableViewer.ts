import * as vscode from 'vscode';
import { BaseViewer } from '../../shared/components/baseViewer';

export class TableViewer extends BaseViewer {
    
    protected getHtmlForWebview(webview: vscode.Webview, document: vscode.TextDocument): string {
        const styleUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'table', 'table.css')
        );
        const scriptUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'table', 'table.js')
        );
        const agGridUri = webview.asWebviewUri(
            vscode.Uri.joinPath(this.context.extensionUri, 'webview', 'resources', 'ag-grid-community.min.js')
        );

        const nonce = this.getNonce();

        return `<!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src ${webview.cspSource} 'unsafe-inline'; script-src ${webview.cspSource} 'nonce-${nonce}' https://unpkg.com;">
            <link href="${styleUri}" rel="stylesheet">
            <title>AppSage Table Viewer</title>
        </head>
        <body>
            <div id="toolbar">
                <button id="exportBtn">Export CSV</button>
                <button id="filterBtn">Toggle Filters</button>
                <input type="text" id="searchInput" placeholder="Search...">
            </div>
            <div id="tableGrid" class="ag-theme-alpine"></div>
            <script nonce="${nonce}" src="https://unpkg.com/ag-grid-community@30.0.0/dist/ag-grid-community.min.js"></script>
            <script nonce="${nonce}" src="${scriptUri}"></script>
        </body>
        </html>`;
    }
}
