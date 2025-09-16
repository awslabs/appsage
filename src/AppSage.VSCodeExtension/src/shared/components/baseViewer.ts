import * as vscode from 'vscode';
import { AppSageLogger } from '../logging';

export abstract class BaseViewer implements vscode.CustomTextEditorProvider {
    protected readonly context: vscode.ExtensionContext;
    protected logger = AppSageLogger.getInstance();
    protected componentLogger = this.logger.forComponent('BaseViewer');

    constructor(context: vscode.ExtensionContext) {
        this.context = context;
    }

    public async resolveCustomTextEditor(
        document: vscode.TextDocument,
        webviewPanel: vscode.WebviewPanel,
        _token: vscode.CancellationToken
    ): Promise<void> {
        webviewPanel.webview.options = {
            enableScripts: true,
            localResourceRoots: [
                vscode.Uri.joinPath(this.context.extensionUri, 'webview')
            ],
            enableCommandUris: false,
            enableForms: false
        };

        webviewPanel.webview.html = this.getHtmlForWebview(webviewPanel.webview, document);

        let webviewReady = false;
        let readyTimeout: NodeJS.Timeout;

        const updateWebview = () => {
            if (webviewReady) {
                webviewPanel.webview.postMessage({
                    type: 'update',
                    content: document.getText(),
                });
            }
        };

        // Set up a timeout to send initial content even if webview doesn't signal ready
        readyTimeout = setTimeout(() => {
            if (!webviewReady) {
                this.componentLogger.warning('Webview ready timeout - sending content anyway');
                webviewReady = true;
                updateWebview();
            }
        }, 5000); // 5 second timeout

        // Handle messages from the webview
        webviewPanel.webview.onDidReceiveMessage((message) => {
            switch (message.type) {
                case 'webview-ready':
                    if (!webviewReady) {
                        webviewReady = true;
                        clearTimeout(readyTimeout);
                        this.componentLogger.info('Webview ready signal received');
                        // Send initial content when webview is ready
                        updateWebview();
                    }
                    break;
                case 'error':
                    // Log errors to centralized logger
                    this.componentLogger.error(`Webview error: ${message.message}`);
                    break;
                default:
                    // Allow derived classes to handle custom messages
                    Promise.resolve(this.handleCustomMessage(message, document, webviewPanel));
                    break;
            }
        });

        const changeDocumentSubscription = vscode.workspace.onDidChangeTextDocument((e: vscode.TextDocumentChangeEvent) => {
            if (e.document.uri.toString() === document.uri.toString()) {
                this.componentLogger.debug('Document changed, updating webview');
                updateWebview();
            }
        });

        webviewPanel.onDidDispose(() => {
            this.componentLogger.info('Webview panel disposed');
            changeDocumentSubscription.dispose();
            clearTimeout(readyTimeout);
        });

        // Don't call updateWebview() immediately - wait for webview to be ready
    }

    protected abstract getHtmlForWebview(webview: vscode.Webview, document: vscode.TextDocument): string;

    protected handleCustomMessage(message: any, document: vscode.TextDocument, webviewPanel: vscode.WebviewPanel): void | Promise<void> {
        // Default implementation does nothing, derived classes can override
    }

    protected getNonce(): string {
        let text = '';
        const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        for (let i = 0; i < 32; i++) {
            text += possible.charAt(Math.floor(Math.random() * possible.length));
        }
        return text;
    }
}
