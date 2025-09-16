import * as path from 'path';

export class FileUtils {
    public static getFileExtension(filePath: string): string {
        return path.extname(filePath);
    }

    public static getFileName(filePath: string): string {
        return path.basename(filePath);
    }

    public static getFileNameWithoutExtension(filePath: string): string {
        const fileName = path.basename(filePath);
        return path.parse(fileName).name;
    }

    public static isAppSageFile(filePath: string): boolean {
        const fileName = this.getFileName(filePath);
        return fileName.includes('.appsage.');
    }

    public static getAppSageFileType(filePath: string): string | null {
        const fileName = this.getFileName(filePath);
        const match = fileName.match(/\.appsage\.(\w+)$/);
        return match ? match[1] : null;
    }

    public static validateFilePath(filePath: string): boolean {
        return typeof filePath === 'string' && filePath.length > 0;
    }
}
