import * as vscode from 'vscode';

export class GraphWebview {
    private panel: vscode.WebviewPanel;
    private context: vscode.ExtensionContext;

    constructor(context: vscode.ExtensionContext, panel: vscode.WebviewPanel) {
        this.context = context;
        this.panel = panel;
    }

    public updateContent(graphData: string): void {
        this.panel.webview.postMessage({
            type: 'updateGraph',
            data: graphData
        });
    }

    public dispose(): void {
        this.panel.dispose();
    }
}
