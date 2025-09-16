import * as vscode from 'vscode';

export class WebviewPanel {
    private static currentPanels: Map<string, vscode.WebviewPanel> = new Map();

    public static createOrShow(
        extensionUri: vscode.Uri,
        viewType: string,
        title: string,
        column: vscode.ViewColumn = vscode.ViewColumn.One
    ): vscode.WebviewPanel {
        const existingPanel = WebviewPanel.currentPanels.get(viewType);
        
        if (existingPanel) {
            existingPanel.reveal();
            return existingPanel;
        }

        const panel = vscode.window.createWebviewPanel(
            viewType,
            title,
            column,
            {
                enableScripts: true,
                retainContextWhenHidden: true,
            }
        );

        panel.onDidDispose(() => {
            WebviewPanel.currentPanels.delete(viewType);
        });

        WebviewPanel.currentPanels.set(viewType, panel);
        return panel;
    }

    public static revive(
        panel: vscode.WebviewPanel,
        extensionUri: vscode.Uri,
        viewType: string
    ): void {
        WebviewPanel.currentPanels.set(viewType, panel);
    }
}
