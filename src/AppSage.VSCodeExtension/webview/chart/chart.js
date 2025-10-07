(function() {
    const vscode = acquireVsCodeApi();
    const logger = new WebViewLogger(vscode, 'ChartMain');
    
    logger.info('=== CHART.JS SCRIPT STARTING ===');
    logger.info('Document ready state:', document.readyState);
    logger.info('Current timestamp:', new Date().toISOString());
    
    // Main application components
    let chartRenderer = null;
    let chartControls = null;
    
    // Current state
    let currentChartData = null;

    function initializeComponents() {
        logger.info('=== INITIALIZATION START ===');
        logger.info('Initializing components...');
        
        // Check if DOM elements exist
        logger.debug('DOM Check - chartContainer exists:', !!document.getElementById('chartContainer'));
        logger.debug('DOM Check - chartTypeSelect exists:', !!document.getElementById('chartTypeSelect'));
        logger.debug('DOM Check - tableSelect exists:', !!document.getElementById('tableSelect'));
        
        // Initialize chart renderer
        logger.info('Initializing ChartRenderer...');
        chartRenderer = new ChartRenderer(vscode);
        if (!chartRenderer.initialize('chartContainer')) {
            logger.error('Failed to initialize ChartRenderer');
            return false;
        }
        logger.info('ChartRenderer initialized successfully');
        
        // Initialize chart controls
        logger.info('Initializing ChartControls...');
        try {
            chartControls = new ChartControls(vscode, chartRenderer);
            logger.info('ChartControls initialized successfully');
        } catch (error) {
            logger.error('ChartControls initialization failed', { error });
            return false;
        }
        
        // Show initial loading state
        chartRenderer.showLoading('Waiting for chart data...');
        
        logger.info('=== INITIALIZATION COMPLETE ===');
        return true;
    }

    function handleMessage(event) {
        const message = event.data;
        logger.debug('Received message', { command: message.command });
        
        switch (message.command) {
            case 'updateChartData':
                handleUpdateChartData(message);
                break;
            case 'tableSummary':
                handleTableSummary(message);
                break;
            case 'error':
                handleError(message);
                break;
            default:
                logger.warning('Unhandled message command', { command: message.command });
                break;
        }
    }

    function handleUpdateChartData(message) {
        try {
            logger.info('Updating chart data', { 
                tableCount: message.data?.tables?.length || 0 
            });
            
            currentChartData = message.data;
            
            if (chartControls) {
                chartControls.updateData(currentChartData);
            }
            
            // Show sidebar with table information
            if (message.summary && message.summary.length > 0) {
                chartControls.showSidebar();
            }
            
            logger.info('Chart data updated successfully');
        } catch (error) {
            logger.error('Failed to handle chart data update', { error });
            showError('Failed to update chart data');
        }
    }

    function handleTableSummary(message) {
        try {
            logger.info('Table summary received', { 
                tableCount: message.summary?.length || 0 
            });
            
            // Update sidebar if visible
            if (chartControls) {
                chartControls.updateSidebar();
            }
        } catch (error) {
            logger.error('Failed to handle table summary', { error });
        }
    }

    function handleError(message) {
        logger.error('Error message received', { error: message.error });
        showError(message.error);
    }

    function showError(message) {
        logger.error('Showing error to user', { message });
        
        if (chartRenderer) {
            chartRenderer.showError(message);
        } else {
            // Fallback error display
            const errorEl = document.getElementById('errorMessage');
            const errorText = document.getElementById('errorText');
            if (errorEl && errorText) {
                errorText.textContent = message;
                errorEl.style.display = 'block';
            }
        }
    }

    function notifyReady() {
        logger.info('Sending ready notification to extension');
        vscode.postMessage({ command: 'chartReady' });
    }

    // Initialize when DOM is ready
    function initialize() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                logger.info('DOM loaded, initializing...');
                if (initializeComponents()) {
                    notifyReady();
                }
            });
        } else {
            logger.info('DOM already loaded, initializing immediately...');
            if (initializeComponents()) {
                notifyReady();
            }
        }
    }

    // Set up message listener
    window.addEventListener('message', handleMessage);
    
    // Handle window resize
    window.addEventListener('resize', () => {
        if (chartRenderer) {
            chartRenderer.resize();
        }
    });

    // Handle visibility change
    document.addEventListener('visibilitychange', () => {
        if (!document.hidden && chartRenderer) {
            // Refresh chart when tab becomes visible
            setTimeout(() => {
                chartRenderer.resize();
            }, 100);
        }
    });

    // Cleanup on unload
    window.addEventListener('beforeunload', () => {
        logger.info('Cleaning up chart components');
        if (chartRenderer) {
            chartRenderer.dispose();
        }
    });

    // Error handling
    window.addEventListener('error', (event) => {
        logger.error('Global error caught', { 
            message: event.message,
            filename: event.filename,
            lineno: event.lineno,
            colno: event.colno,
            error: event.error
        });
        
        showError('An unexpected error occurred. Please check the console for details.');
    });

    window.addEventListener('unhandledrejection', (event) => {
        logger.error('Unhandled promise rejection', { 
            reason: event.reason 
        });
        
        showError('An unexpected error occurred. Please check the console for details.');
    });

    // Start initialization
    logger.info('Starting chart application initialization...');
    initialize();

    // Expose debug functions in development
    if (typeof DEBUG !== 'undefined' && DEBUG) {
        window.chartDebug = {
            getRenderer: () => chartRenderer,
            getControls: () => chartControls,
            getCurrentData: () => currentChartData,
            showError: showError,
            logger: logger
        };
    }

    logger.info('=== CHART.JS SCRIPT LOADED ===');
})();
