import * as vscode from 'vscode';

export class TableWebview {
    private panel: vscode.WebviewPanel;
    private context: vscode.ExtensionContext;

    constructor(context: vscode.ExtensionContext, panel: vscode.WebviewPanel) {
        this.context = context;
        this.panel = panel;
    }

    public updateContent(tableData: string): void {
        this.panel.webview.postMessage({
            type: 'updateTable',
            data: tableData
        });
    }

    public dispose(): void {
        this.panel.dispose();
    }
}
