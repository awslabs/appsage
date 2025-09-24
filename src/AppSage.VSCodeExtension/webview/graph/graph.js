(function() {
    console.log('=== GRAPH.JS SCRIPT STARTING ===');
    console.log('Document ready state:', document.readyState);
    console.log('Current timestamp:', new Date().toISOString());
    
    const vscode = acquireVsCodeApi();
    
    // Main application components
    let graphRenderer = null;
    let topMenu = null;
    let sidePanel = null;
    
    // Enhanced view components
    let graphCustomization = null;
    let enhancedViewCustomizer = null;
    
    // Current state
    let currentGraphData = null;

    function initializeComponents() {
        console.log('=== INITIALIZATION START ===');
        console.log('Initializing components...');
        
        // Check if DOM elements exist
        console.log('DOM Check - sidebar-panel exists:', !!document.getElementById('sidebar-panel'));
        console.log('DOM Check - showSidePanel exists:', !!document.getElementById('showSidePanel'));
        console.log('DOM Check - cy exists:', !!document.getElementById('cy'));
        
        // Initialize enhanced view components first
        if (!initializeEnhancedView()) {
            console.warn('Enhanced view components not available');
        }
        
        // Initialize graph renderer
        console.log('Initializing GraphRenderer...');
        graphRenderer = new GraphRenderer(vscode, graphCustomization, enhancedViewCustomizer);
        if (!graphRenderer.initialize()) {
            console.error('Failed to initialize GraphRenderer');
            return false;
        }
        console.log('GraphRenderer initialized successfully');
        
        // Initialize side panel
        console.log('Initializing SidePanel...');
        try {
            sidePanel = new SidePanel(vscode, enhancedViewCustomizer);
            console.log('SidePanel initialized, sidebarOpen:', sidePanel.sidebarOpen);
        } catch (error) {
            console.error('Failed to initialize SidePanel:', error);
            return false;
        }
        
        // Initialize top menu
        console.log('Initializing TopMenu...');
        try {
            topMenu = new TopMenu(graphRenderer, sidePanel);
            topMenu.setCoseBilkentAvailable(graphRenderer.getCoseBilkentAvailable());
            console.log('TopMenu initialized successfully');
        } catch (error) {
            console.error('Failed to initialize TopMenu:', error);
            return false;
        }
        
        // Set up event handlers between components
        setupComponentInteractions();
        
        console.log('All components initialized successfully');
        
        // Signal to VS Code that the webview is ready to receive data
        if (typeof vscode !== 'undefined') {
            vscode.postMessage({
                type: 'webview-ready'
            });
        }
        
        return true;
    }

    function setupComponentInteractions() {
        // Set up graph renderer event handlers
        graphRenderer.setEventHandlers(
            (nodeData) => sidePanel.displayNodeProperties(nodeData),  // onNodeSelected
            (edgeData) => sidePanel.displayEdgeProperties(edgeData),  // onEdgeSelected
            () => sidePanel.clearPropertyDisplay()                    // onSelectionCleared
        );
    }

    function initializeEnhancedView() {
        console.log('Initializing Enhanced View...');
        
        // Initialize graph customization
        if (typeof GraphCustomization !== 'undefined') {
            try {
                graphCustomization = new GraphCustomization();
                console.log('GraphCustomization initialized successfully');
            } catch (error) {
                console.error('Failed to initialize GraphCustomization:', error);
                return false;
            }
        } else {
            console.error('GraphCustomization class not available');
            return false;
        }

        // Initialize enhanced view customizer
        if (typeof EnhancedViewCustomizer !== 'undefined' && graphCustomization) {
            try {
                enhancedViewCustomizer = new EnhancedViewCustomizer(vscode, graphCustomization);
                enhancedViewCustomizer.initialize(); // Initialize the customizer
                console.log('EnhancedViewCustomizer initialized successfully');
                // Load saved customization
                enhancedViewCustomizer.loadFromState();
            } catch (error) {
                console.error('Failed to initialize EnhancedViewCustomizer:', error);
                return false;
            }
        } else {
            console.error('EnhancedViewCustomizer class not available or GraphCustomization failed');
            return false;
        }

        return true;
    }    function updateGraph(graphData) {
        if (!graphRenderer || !graphRenderer.getIsInitialized()) {
            console.warn('Graph renderer not initialized yet, delaying graph update');
            setTimeout(() => updateGraph(graphData), 100);
            return;
        }

        try {
            console.log('Parsing graph data...');
            const graph = JSON.parse(graphData);
            console.log('Graph data parsed successfully');
            
            const validationResult = validateGraphData(graph);
            if (!validationResult.isValid) {
                showError(validationResult.errorMessage);
                return;
            }

            const { validNodes, validEdges } = processGraphData(graph);
            
            if (validNodes.length === 0) {
                showError('No valid nodes found. Please check the graph data format.');
                return;
            }

            // Update types in top menu
            if (topMenu && typeof topMenu.updateTypes === 'function') {
                topMenu.updateTypes(validNodes, validEdges);
            } else {
                console.warn('TopMenu not available or updateTypes method missing');
            }
            
            // Update legend in side panel (legend now gets data from customization settings)
            if (sidePanel && typeof sidePanel.updateLegend === 'function') {
                sidePanel.updateLegend();
            } else {
                console.warn('SidePanel not available or updateLegend method missing');
            }
            
            // Update the graph
            const success = graphRenderer.updateGraph(validNodes, validEdges);
            if (success) {
                console.log(`Graph updated successfully: ${validNodes.length} nodes, ${validEdges.length} edges`);
            }
            
        } catch (error) {
            if (error instanceof SyntaxError) {
                console.error('JSON parsing error:', error.message);
                showError(`Invalid JSON format: ${error.message}`);
            } else {
                console.error('Error parsing graph data:', error);
                showError('Error processing graph data. Please check the console for details.');
            }
        }
    }

    function validateGraphData(graph) {
        if (!graph || typeof graph !== 'object') {
            return {
                isValid: false,
                errorMessage: 'Invalid graph data format'
            };
        }

        if (!graph.Nodes || !Array.isArray(graph.Nodes)) {
            return {
                isValid: false,
                errorMessage: 'Graph data missing nodes array'
            };
        }

        if (!graph.Edges || !Array.isArray(graph.Edges)) {
            return {
                isValid: false,
                errorMessage: 'Graph data missing edges array'
            };
        }

        return { isValid: true };
    }

    function processGraphData(graph) {
        // Helper function to extract ID from source/target (can be string or object)
        const extractId = (sourceOrTarget) => {
            if (typeof sourceOrTarget === 'string') {
                return sourceOrTarget;
            } else if (sourceOrTarget && typeof sourceOrTarget === 'object') {
                // Check for Id (capital I) first, then id (lowercase)
                const id = sourceOrTarget.Id || sourceOrTarget.id;
                if (id && typeof id === 'string' && id.trim().length > 0) {
                    return id.trim();
                }
            }
            return null;
        };

        // Helper function to log errors to VS Code output
        const logError = (message) => {
            console.error(message);
            if (typeof vscode !== 'undefined') {
                vscode.postMessage({
                    type: 'error',
                    message: message
                });
            }
        };

        // Validate and process nodes
        const nodeIds = new Set();
        const validNodes = [];
        let nodeErrors = 0;

        for (let i = 0; i < graph.Nodes.length; i++) {
            const node = graph.Nodes[i];
            
            // Check if node exists and has valid ID
            if (!node || !node.Id || typeof node.Id !== 'string') {
                logError(`Node at index ${i} missing or invalid Id property: ${JSON.stringify(node)}`);
                nodeErrors++;
                continue;
            }

            // Check for duplicate node IDs
            if (nodeIds.has(node.Id)) {
                logError(`Duplicate node Id found: "${node.Id}" at index ${i}`);
                nodeErrors++;
                continue;
            }

            // Valid node
            nodeIds.add(node.Id);
            validNodes.push({
                data: {
                    id: node.Id,
                    name: node.Name || node.Id,
                    type: node.Type || 'default'
                }
            });
        }

        // Validate and process edges
        const edgeIds = new Set();
        const validEdges = [];
        let edgeErrors = 0;

        for (let i = 0; i < graph.Edges.length; i++) {
            const edge = graph.Edges[i];
            
            // Check if edge exists and has valid ID
            if (!edge || !edge.Id || typeof edge.Id !== 'string') {
                logError(`Edge at index ${i} missing or invalid Id property: ${JSON.stringify(edge)}`);
                edgeErrors++;
                continue;
            }

            // Check for duplicate edge IDs
            if (edgeIds.has(edge.Id)) {
                logError(`Duplicate edge Id found: "${edge.Id}" at index ${i}`);
                edgeErrors++;
                continue;
            }

            // Extract and validate source and target IDs
            const sourceId = extractId(edge.Source);
            const targetId = extractId(edge.Target);

            if (!sourceId || !targetId) {
                logError(`Edge "${edge.Id}" at index ${i} has invalid source or target`);
                edgeErrors++;
                continue;
            }

            // Check if source and target nodes exist
            if (!nodeIds.has(sourceId) || !nodeIds.has(targetId)) {
                logError(`Edge "${edge.Id}" references non-existent nodes: "${sourceId}" or "${targetId}"`);
                edgeErrors++;
                continue;
            }

            // Valid edge
            edgeIds.add(edge.Id);
            validEdges.push({
                data: {
                    id: edge.Id,
                    source: sourceId,
                    target: targetId,
                    type: edge.Type || 'default'
                }
            });
        }

        // Log validation summary
        if (nodeErrors > 0 || edgeErrors > 0) {
            logError(`Graph validation completed with ${nodeErrors} node errors and ${edgeErrors} edge errors`);
        } else {
            console.log(`Graph validation passed: ${validNodes.length} valid nodes, ${validEdges.length} valid edges`);
        }

        return { validNodes, validEdges };
    }

    function showError(message) {
        if (graphRenderer) {
            graphRenderer.showError(message);
        }
    }

    // Handle messages from VS Code
    window.addEventListener('message', event => {
        const message = event.data;
        switch (message.type) {
            case 'update':
                updateGraph(message.content);
                // Forward the graph data to the extension for property panel
                vscode.postMessage({
                    type: 'update',
                    content: message.content
                });
                break;
            case 'customizationLoaded':
                if (enhancedViewCustomizer && message.data) {
                    enhancedViewCustomizer.applyLoadedCustomization(message.data);
                    if (topMenu.getCurrentViewMode() === 'enhanced') {
                        graphRenderer.applyEnhancedView();
                    }
                }
                break;
            default:
                console.log('Unknown message type:', message.type);
        }
    });

    // Wait for external libraries to load before initializing
    function waitForCytoscape(maxAttempts = 50, currentAttempt = 0) {
        console.log('=== WAITING FOR CYTOSCAPE ===');
        console.log(`waitForCytoscape attempt ${currentAttempt + 1}/${maxAttempts}`);
        console.log('Cytoscape available:', typeof cytoscape !== 'undefined');
        
        if (typeof cytoscape !== 'undefined') {
            console.log('Cytoscape loaded, initializing components...');
            initializeComponents();
        } else if (currentAttempt < maxAttempts) {
            console.log(`Waiting for Cytoscape to load... (attempt ${currentAttempt + 1}/${maxAttempts})`);
            setTimeout(() => waitForCytoscape(maxAttempts, currentAttempt + 1), 100);
        } else {
            console.error('Failed to load Cytoscape after maximum attempts');
            showError('Failed to load graph visualization library. Please refresh the editor.');
            
            // Signal ready even on failure so extension doesn't hang
            if (typeof vscode !== 'undefined') {
                vscode.postMessage({
                    type: 'webview-ready'
                });
            }
        }
    }

    // Start initialization
    waitForCytoscape();

})();
