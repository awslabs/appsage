import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Utility class for loading and processing HTML templates for webviews.
 */
export class TemplateLoader {
    /**
     * Loads an HTML template file and processes placeholders.
     * @param context VS Code extension context
     * @param templatePath Relative path to template from the webview folder
     * @param webview The webview instance for generating URIs
     * @param replacements Object containing placeholder replacements
     * @returns Processed HTML content
     */
    public static loadTemplate(
        context: vscode.ExtensionContext,
        templatePath: string,
        webview: vscode.Webview,
        replacements: Record<string, string> = {}
    ): string {
        const templateFullPath = path.join(context.extensionPath, 'webview', templatePath);
        
        if (!fs.existsSync(templateFullPath)) {
            throw new Error(`Template file not found: ${templateFullPath}`);
        }

        let htmlContent = fs.readFileSync(templateFullPath, 'utf8');

        // Process all replacements
        for (const [placeholder, value] of Object.entries(replacements)) {
            const placeholderPattern = new RegExp(`{{${placeholder}}}`, 'g');
            htmlContent = htmlContent.replace(placeholderPattern, value);
        }

        return htmlContent;
    }

    /**
     * Generates a random nonce for CSP.
     * @returns Random nonce string
     */
    public static generateNonce(): string {
        let text = '';
        const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        for (let i = 0; i < 32; i++) {
            text += possible.charAt(Math.floor(Math.random() * possible.length));
        }
        return text;
    }

    /**
     * Creates a Content Security Policy string for the webview.
     * @param webview The webview instance
     * @param nonce The nonce for scripts
     * @returns CSP string
     */
    public static createCSP(webview: vscode.Webview, nonce: string): string {
        return `default-src 'none'; style-src ${webview.cspSource} 'unsafe-inline'; script-src ${webview.cspSource} 'nonce-${nonce}' https://unpkg.com; img-src ${webview.cspSource} data: https:; font-src ${webview.cspSource} data:; connect-src ${webview.cspSource} https:;`;
    }
}
