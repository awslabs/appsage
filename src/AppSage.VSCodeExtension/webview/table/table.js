(function() {
    const vscode = acquireVsCodeApi();

    function initialize() {
        console.log('Table viewer initialized with static content');
        setupControls();
        
        // Signal to VS Code that the webview is ready
        vscode.postMessage({
            type: 'webview-ready'
        });
    }

    function setupControls() {
        const exportBtn = document.getElementById('exportBtn');
        const filterBtn = document.getElementById('filterBtn');
        const searchInput = document.getElementById('searchInput');
        
        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                console.log('Export button clicked - future implementation');
                // TODO: Implement CSV export for pure HTML table
            });
        }

        if (filterBtn) {
            filterBtn.addEventListener('click', () => {
                console.log('Filter button clicked - future implementation');
                // TODO: Implement filtering for pure HTML table
            });
        }

        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                console.log('Search input changed - future implementation:', e.target.value);
                // TODO: Implement search functionality for pure HTML table
            });
        }
    }

    window.addEventListener('message', event => {
        const message = event.data;
        console.log('Received message:', message.type);
        
        switch (message.type) {
            case 'update':
                console.log('Update message received - future implementation will render dynamic table');
                // TODO: Future implementation will parse tableData and render dynamic HTML table
                break;
            default:
                console.log('Unknown message type:', message.type);
                break;
        }
    });

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

    console.log('Table viewer script loaded - using static HTML table');
})();
