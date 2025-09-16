export class Logger {
    private static outputChannel: any;

    public static initialize(): void {
        // Will be initialized with vscode.window.createOutputChannel
    }

    public static info(message: string): void {
        console.log(`[AppSage] INFO: ${message}`);
        this.writeToChannel(`INFO: ${message}`);
    }

    public static warn(message: string): void {
        console.warn(`[AppSage] WARN: ${message}`);
        this.writeToChannel(`WARN: ${message}`);
    }

    public static error(message: string, error?: Error): void {
        console.error(`[AppSage] ERROR: ${message}`, error);
        this.writeToChannel(`ERROR: ${message}${error ? ` - ${error.message}` : ''}`);
    }

    public static debug(message: string): void {
        console.debug(`[AppSage] DEBUG: ${message}`);
        this.writeToChannel(`DEBUG: ${message}`);
    }

    private static writeToChannel(message: string): void {
        if (this.outputChannel) {
            this.outputChannel.appendLine(`[${new Date().toISOString()}] ${message}`);
        }
    }

    public static setOutputChannel(channel: any): void {
        this.outputChannel = channel;
    }
}
