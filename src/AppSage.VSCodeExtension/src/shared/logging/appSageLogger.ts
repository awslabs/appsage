import * as vscode from 'vscode';

export enum LogLevel {
    Error = 'ERROR',
    Warning = 'WARNING', 
    Info = 'INFO',
    Debug = 'DEBUG'
}

export interface ILogger {
    error(message: string, ...args: any[]): void;
    warning(message: string, ...args: any[]): void;
    info(message: string, ...args: any[]): void;
    debug(message: string, ...args: any[]): void;
    dispose(): void;
}

export class AppSageLogger implements ILogger {
    private static instance: AppSageLogger;
    private outputChannel: vscode.OutputChannel;
    private readonly channelName = 'AppSage';

    private constructor() {
        this.outputChannel = vscode.window.createOutputChannel(this.channelName);
    }

    public static getInstance(): AppSageLogger {
        if (!AppSageLogger.instance) {
            AppSageLogger.instance = new AppSageLogger();
        }
        return AppSageLogger.instance;
    }

    public static initialize(context: vscode.ExtensionContext): AppSageLogger {
        const logger = AppSageLogger.getInstance();
        context.subscriptions.push(logger.outputChannel);
        return logger;
    }

    private formatMessage(level: LogLevel, message: string, component?: string): string {
        const timestamp = new Date().toISOString();
        const componentPrefix = component ? `[${component}]` : '';
        return `${timestamp} ${level} ${componentPrefix} ${message}`;
    }

    private log(level: LogLevel, message: string, component?: string, ...args: any[]): void {
        const formattedMessage = this.formatMessage(level, message, component);
        
        // Log to output channel - use safe concatenation
        if (args.length > 0) {
            const safeArgsString = JSON.stringify(args);
            this.outputChannel.appendLine(formattedMessage + ' ' + safeArgsString);
        } else {
            this.outputChannel.appendLine(formattedMessage);
        }

        // Also log to console for development - use safe format with literal format strings
        const logPrefix = component ? `[${component}]` : '';
        switch (level) {
            case LogLevel.Error:
                if (args.length > 0) {
                    console.error('%s %s', logPrefix, message, ...args);
                } else {
                    console.error('%s %s', logPrefix, message);
                }
                break;
            case LogLevel.Warning:
                if (args.length > 0) {
                    console.warn('%s %s', logPrefix, message, ...args);
                } else {
                    console.warn('%s %s', logPrefix, message);
                }
                break;
            case LogLevel.Info:
                if (args.length > 0) {
                    console.info('%s %s', logPrefix, message, ...args);
                } else {
                    console.info('%s %s', logPrefix, message);
                }
                break;
            case LogLevel.Debug:
                if (args.length > 0) {
                    console.log('%s %s', logPrefix, message, ...args);
                } else {
                    console.log('%s %s', logPrefix, message);
                }
                break;
        }
    }

    public error(message: string, component?: string, ...args: any[]): void {
        this.log(LogLevel.Error, message, component, ...args);
    }

    public warning(message: string, component?: string, ...args: any[]): void {
        this.log(LogLevel.Warning, message, component, ...args);
    }

    public info(message: string, component?: string, ...args: any[]): void {
        this.log(LogLevel.Info, message, component, ...args);
    }

    public debug(message: string, component?: string, ...args: any[]): void {
        this.log(LogLevel.Debug, message, component, ...args);
    }

    public show(): void {
        this.outputChannel.show();
    }

    public clear(): void {
        this.outputChannel.clear();
    }

    public dispose(): void {
        this.outputChannel.dispose();
    }

    // Convenience methods for different components
    public forComponent(componentName: string) {
        return {
            error: (message: string, ...args: any[]) => this.error(message, componentName, ...args),
            warning: (message: string, ...args: any[]) => this.warning(message, componentName, ...args),
            info: (message: string, ...args: any[]) => this.info(message, componentName, ...args),
            debug: (message: string, ...args: any[]) => this.debug(message, componentName, ...args)
        };
    }
}
