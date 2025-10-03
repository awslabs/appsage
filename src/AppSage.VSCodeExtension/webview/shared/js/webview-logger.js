/**
 * WebView Logger - Sends log messages to the VS Code extension for proper logging
 * This allows webview components to use structured logging through the extension's AppSageLogger
 */

class WebViewLogger {
    constructor(vscode, component = '') {
        this.vscode = vscode;
        this.component = component;
    }

    /**
     * Send a log message to the VS Code extension and always log to console safely
     * @param {string} level - The log level (error, warning, info, debug)
     * @param {string} message - The log message
     * @param {...any} args - Additional arguments to log
     */
    _sendLog(level, message, ...args) {
        // Always send to VS Code extension for centralized logging
        if (this.vscode && typeof this.vscode.postMessage === 'function') {
            this.vscode.postMessage({
                type: 'webview-log',
                level: level,
                message: message,
                component: this.component,
                args: args.length > 0 ? JSON.stringify(args) : undefined,
                timestamp: new Date().toISOString()
            });
        }
        
        // Always log to console safely using literal format strings
        const logPrefix = this.component ? `[${this.component}]` : '';
        switch (level) {
            case 'error':
                if (args.length > 0) {
                    console.error('%s %s', logPrefix, message, ...args);
                } else {
                    console.error('%s %s', logPrefix, message);
                }
                break;
            case 'warning':
                if (args.length > 0) {
                    console.warn('%s %s', logPrefix, message, ...args);
                } else {
                    console.warn('%s %s', logPrefix, message);
                }
                break;
            case 'info':
                if (args.length > 0) {
                    console.info('%s %s', logPrefix, message, ...args);
                } else {
                    console.info('%s %s', logPrefix, message);
                }
                break;
            case 'debug':
                if (args.length > 0) {
                    console.log('%s %s', logPrefix, message, ...args);
                } else {
                    console.log('%s %s', logPrefix, message);
                }
                break;
        }
    }

    /**
     * Log an error message
     * @param {string} message - The error message
     * @param {...any} args - Additional arguments
     */
    error(message, ...args) {
        this._sendLog('error', message, ...args);
    }

    /**
     * Log a warning message
     * @param {string} message - The warning message
     * @param {...any} args - Additional arguments
     */
    warning(message, ...args) {
        this._sendLog('warning', message, ...args);
    }

    /**
     * Log an info message
     * @param {string} message - The info message
     * @param {...any} args - Additional arguments
     */
    info(message, ...args) {
        this._sendLog('info', message, ...args);
    }

    /**
     * Log a debug message
     * @param {string} message - The debug message
     * @param {...any} args - Additional arguments
     */
    debug(message, ...args) {
        this._sendLog('debug', message, ...args);
    }

    /**
     * Create a logger for a specific component
     * @param {string} componentName - The name of the component
     * @returns {WebViewLogger} A new logger instance for the component
     */
    forComponent(componentName) {
        return new WebViewLogger(this.vscode, componentName);
    }
}

// Make it globally available
if (typeof window !== 'undefined') {
    window.WebViewLogger = WebViewLogger;
}

// Export for modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = WebViewLogger;
}
