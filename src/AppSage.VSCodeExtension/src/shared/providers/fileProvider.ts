import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';

export class FileProvider {
    public static async readFile(uri: vscode.Uri): Promise<string> {
        try {
            const content = await vscode.workspace.fs.readFile(uri);
            return Buffer.from(content).toString('utf8');
        } catch (error) {
            throw new Error(`Failed to read file: ${error}`);
        }
    }

    public static async writeFile(uri: vscode.Uri, content: string): Promise<void> {
        try {
            const contentBuffer = Buffer.from(content, 'utf8');
            await vscode.workspace.fs.writeFile(uri, contentBuffer);
        } catch (error) {
            throw new Error(`Failed to write file: ${error}`);
        }
    }

    public static getFileExtension(uri: vscode.Uri): string {
        return path.extname(uri.fsPath);
    }

    public static isAppSageFile(uri: vscode.Uri): boolean {
        const fileName = path.basename(uri.fsPath);
        return fileName.includes('.appsage');
    }

    public static getAppSageFileType(uri: vscode.Uri): string | null {
        const fileName = path.basename(uri.fsPath);
        const match = fileName.match(/\.appsage(\w+)$/);
        return match ? match[1] : null;
    }
}
