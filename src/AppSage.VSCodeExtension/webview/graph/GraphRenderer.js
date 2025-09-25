/**
 * GraphRenderer - Core Cytoscape functionality and rendering logic
 */
class GraphRenderer {
    constructor(vscode, graphCustomization, enhancedViewCustomizer) {
        this.vscode = vscode;
        this.graphCustomization = graphCustomization;
        this.enhancedViewCustomizer = enhancedViewCustomizer;
        this.cy = null;
        this.isInitialized = false;
        this.coseBilkentAvailable = false;
        this.currentViewMode = 'basic';
        this.originalNodes = [];
        this.originalEdges = [];
        
        this.onNodeSelected = null;
        this.onEdgeSelected = null;
        this.onSelectionCleared = null;
    }

    checkLibraries() {
        // Check Cytoscape
        if (typeof cytoscape === 'undefined') {
            console.error('Cytoscape library not loaded');
            return false;
        }

        // Check cose-bilkent extension with better error handling
        if (typeof cytoscapeCoseBilkent !== 'undefined') {
            try {
                // Ensure cytoscape is properly initialized before registering extension
                if (cytoscape && typeof cytoscape.use === 'function') {
                    cytoscape.use(cytoscapeCoseBilkent);
                    this.coseBilkentAvailable = true;
                    console.log('Cose-bilkent extension registered successfully');
                } else {
                    console.warn('Cytoscape.use method not available, skipping cose-bilkent registration');
                }
            } catch (error) {
                console.warn('Failed to register cose-bilkent extension:', error);
                this.coseBilkentAvailable = false;
            }
        } else {
            console.warn('Cose-bilkent extension not available - some layouts may not work');
            this.coseBilkentAvailable = false;
        }

        return true;
    }

    initialize() {
        if (!this.checkLibraries()) {
            return false;
        }

        try {
            const container = document.getElementById('cy');
            if (!container) {
                console.error('Graph container element with id "cy" not found');
                return false;
            }

            this.cy = cytoscape({
                container: container,
                style: this.getBasicStyles(),
                layout: this.getDefaultLayout()
            });

            this.isInitialized = true;
            this.setupEventHandlers();
            this.setupResizeHandler();
            
            console.log('Cytoscape initialized successfully');
            
            // Expose functions globally for enhanced view customizer
            window.applyEnhancedView = () => this.applyEnhancedView();
            window.applyViewMode = () => this.applyViewMode();
            console.log('Enhanced view functions exposed globally');
            
            // Listen for customization changes
            document.addEventListener('customizationChanged', (event) => {
                console.log('Customization changed event received:', event.detail);
                if (this.currentViewMode === 'enhanced') {
                    console.log('Applying enhanced view due to customization change');
                    this.applyEnhancedView();
                }
            });
            
            return true;
        } catch (error) {
            console.error('Error initializing Cytoscape:', error);
            this.showError('Failed to initialize graph visualization. Please refresh the editor.');
            return false;
        }
    }

    setupEventHandlers() {
        if (!this.cy) return;

        // Node selection
        this.cy.on('tap', 'node', (event) => {
            const node = event.target;
            const nodeId = node.data('id');
            const nodeData = node.data();
            console.log('Node selected:', nodeId, 'Full node data:', nodeData);
            
            if (this.onNodeSelected) {
                this.onNodeSelected(nodeData);
            }
            
            // Send node selection to extension
            this.vscode.postMessage({
                type: 'nodeSelected',
                nodeId: nodeId
            });
        });

        // Edge selection
        this.cy.on('tap', 'edge', (event) => {
            const edge = event.target;
            const edgeId = edge.data('id');
            const edgeData = edge.data();
            console.log('Edge selected:', edgeId, 'Full edge data:', edgeData);
            
            if (this.onEdgeSelected) {
                this.onEdgeSelected(edgeData);
            }
            
            // Send edge selection to extension
            this.vscode.postMessage({
                type: 'edgeSelected',
                edgeId: edgeId
            });
        });

        // Node double-click for file opening
        this.cy.on('dbltap', 'node', (event) => {
            const node = event.target;
            const nodeData = node.data();
            console.log('Node double-clicked:', nodeData.id, 'Full node data:', nodeData);
            
            // Check if the node has ResourceFilePath attribute in the Attributes object
            const resourceFilePath = nodeData.Attributes && nodeData.Attributes.ResourceFilePath;
            if (resourceFilePath && typeof resourceFilePath === 'string') {
                console.log('Opening file:', resourceFilePath);
                
                // Send message to VS Code extension to open the file
                this.vscode.postMessage({
                    type: 'openFile',
                    filePath: resourceFilePath,
                    nodeId: nodeData.id
                });
            } else {
                console.log('Node does not have ResourceFilePath attribute in Attributes object');
                console.log('Available attributes:', nodeData.Attributes);
            }
        });

        // Clear selection when clicking on background
        this.cy.on('tap', (event) => {
            // Only clear if tapping on background (not on nodes or edges)
            if (event.target === this.cy) {
                console.log('Background clicked - clearing selection');
                
                if (this.onSelectionCleared) {
                    this.onSelectionCleared();
                }
                
                this.vscode.postMessage({
                    type: 'selectionCleared'
                });
            }
        });
    }

    setupResizeHandler() {
        // Listen for sidebar toggle events
        document.addEventListener('sidebarToggled', () => {
            if (this.cy && this.isInitialized) {
                setTimeout(() => {
                    this.cy.resize();
                    this.cy.fit();
                }, 300);
            }
        });
    }

    getBasicStyles() {
        return [
            {
                selector: 'node',
                style: {
                    'background-color': '#666',
                    'label': 'data(name)',
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'color': '#fff',
                    'font-size': '12px',
                    'width': '60px',
                    'height': '60px',
                    'text-wrap': 'wrap',
                    'text-max-width': '50px'
                }
            },
            {
                selector: 'node[type="Project"]',
                style: {
                    'background-color': '#4a90e2',
                    'shape': 'rectangle'
                }
            },
            {
                selector: 'node[type="Repository"]',
                style: {
                    'background-color': '#7ed321',
                    'shape': 'ellipse'
                }
            },
            {
                selector: 'node[type="Assembly"]',
                style: {
                    'background-color': '#f5a623',
                    'shape': 'triangle'
                }
            },
            {
                selector: 'edge',
                style: {
                    'width': 2,
                    'line-color': '#ccc',
                    'target-arrow-color': '#ccc',
                    'target-arrow-shape': 'triangle',
                    'curve-style': 'bezier',
                    'arrow-scale': 1.2
                }
            },
            {
                selector: 'node:selected',
                style: {
                    'background-color': '#ff6b6b',
                    'border-color': '#ff4757',
                    'border-width': '3px'
                }
            }
        ];
    }

    getDefaultLayout() {
        return {
            name: 'cose',
            idealEdgeLength: 100,
            nodeOverlap: 20,
            refresh: 20,
            fit: true,
            padding: 30,
            randomize: false,
            componentSpacing: 100,
            nodeRepulsion: 400000,
            edgeElasticity: 100,
            nestingFactor: 5,
            gravity: 80,
            numIter: 1000,
            initialTemp: 200,
            coolingFactor: 0.95,
            minTemp: 1.0,
            animate: true,
            animationDuration: 500
        };
    }

    setViewMode(mode) {
        this.currentViewMode = mode;
        this.applyViewMode();
    }

    applyViewMode() {
        console.log('Applying view mode:', this.currentViewMode);
        
        if (!this.cy || !this.isInitialized) {
            console.warn('Cytoscape not initialized, cannot apply view mode');
            return;
        }

        if (this.currentViewMode === 'basic') {
            console.log('Applying basic view');
            this.applyBasicView();
        } else if (this.currentViewMode === 'enhanced') {
            console.log('Applying enhanced view');
            this.applyEnhancedView();
        }
        
        console.log('View mode applied successfully');
    }

    applyBasicView() {
        if (!this.cy) return;
        
        const basicStyles = this.getBasicStyles();
        this.cy.style(basicStyles);
    }

    applyEnhancedView() {
        console.log('Starting applyEnhancedView...');
        
        if (!this.graphCustomization) {
            console.error('GraphCustomization not available for enhanced view');
            return;
        }

        console.log('GraphCustomization available, proceeding...');
        
        const enhancedStyles = [];
        const nodes = this.cy.nodes();
        const edges = this.cy.edges();

        console.log(`Processing ${nodes.length} nodes and ${edges.length} edges`);

        // Build dynamic size maps for nodes that use dynamic sizing
        const dynamicSizeMaps = new Map();
        const nodeTypes = this.graphCustomization.getAllNodeTypes();
        
        console.log('Available node types:', nodeTypes);
        
        nodeTypes.forEach(nodeType => {
            const customization = this.graphCustomization.getNodeCustomization(nodeType);
            console.log(`Node type ${nodeType} customization:`, customization);
            
            if (customization.sizeMode === 'dynamic' && customization.dynamicKey) {
                // Convert Cytoscape nodes to the format expected by calculateDynamicSize
                const nodeDataArray = [];
                nodes.forEach(node => {
                    if (node.data('type') === nodeType) {
                        nodeDataArray.push({ 
                            data: node.data(),
                            id: node.data('id')
                        });
                    }
                });
                
                console.log(`Found ${nodeDataArray.length} nodes of type ${nodeType} for dynamic sizing`);
                
                if (nodeDataArray.length > 0) {
                    const sizeMap = this.graphCustomization.calculateDynamicSize(
                        nodeDataArray, 
                        nodeType, 
                        customization.dynamicKey
                    );
                    dynamicSizeMaps.set(nodeType, sizeMap);
                    console.log(`Dynamic size map for ${nodeType}:`, sizeMap);
                }
            }
        });

        // Base node style
        enhancedStyles.push({
            selector: 'node',
            style: {
                'label': 'data(name)',
                'text-valign': 'center',
                'text-halign': 'center',
                'color': '#fff',
                'font-size': '12px',
                'text-wrap': 'wrap',
                'text-max-width': '50px'
            }
        });

        // Apply node customizations
        console.log('Applying node customizations...');
        nodeTypes.forEach(nodeType => {
            const customization = this.graphCustomization.getNodeCustomization(nodeType);
            const selector = `node[type="${nodeType}"]`;
            
            console.log(`Creating style for ${selector}:`, customization);
            
            const style = {
                'background-color': customization.color,
                'shape': customization.shape
            };

            // Handle sizing
            if (customization.sizeMode === 'static') {
                style.width = `${customization.staticSize}px`;
                style.height = `${customization.staticSize}px`;
            } else if (customization.sizeMode === 'dynamic' && dynamicSizeMaps.has(nodeType)) {
                // For dynamic sizing, we'll need to apply sizes individually
                const sizeMap = dynamicSizeMaps.get(nodeType);
                if (sizeMap && sizeMap.size > 0) {
                    sizeMap.forEach((size, nodeId) => {
                        enhancedStyles.push({
                            selector: `node[id="${nodeId}"]`,
                            style: {
                                'width': `${size}px`,
                                'height': `${size}px`
                            }
                        });
                    });
                } else {
                    // Fallback to static size if dynamic sizing fails
                    style.width = `${customization.staticSize}px`;
                    style.height = `${customization.staticSize}px`;
                }
            } else {
                // Default size if no sizing mode specified
                style.width = '50px';
                style.height = '50px';
            }

            enhancedStyles.push({ selector, style });
        });

        // Apply edge customizations
        console.log('Applying edge customizations...');
        const edgeTypes = this.graphCustomization.getAllEdgeTypes();
        console.log('Available edge types:', edgeTypes);
        
        edgeTypes.forEach(edgeType => {
            const customization = this.graphCustomization.getEdgeCustomization(edgeType);
            const selector = `edge[type="${edgeType}"]`;
            
            console.log(`Creating edge style for ${selector}:`, customization);
            
            const style = {
                'line-color': customization.color,
                'target-arrow-color': customization.color,
                'target-arrow-shape': customization.arrow,
                'width': customization.width,
                'curve-style': 'bezier'
            };

            // Handle edge style (solid, dashed, dotted)
            if (customization.style === 'dashed') {
                style['line-style'] = 'dashed';
            } else if (customization.style === 'dotted') {
                style['line-style'] = 'dotted';
            } else {
                style['line-style'] = 'solid';
            }

            enhancedStyles.push({ selector, style });
        });

        // Find all actual node types in the graph and apply default styles for unmatched ones
        const actualNodeTypes = new Set();
        nodes.forEach(node => {
            const nodeType = node.data('type');
            if (nodeType) {
                actualNodeTypes.add(nodeType);
            }
        });

        // Apply default styles for unmatched node types
        actualNodeTypes.forEach(nodeType => {
            if (!nodeTypes.includes(nodeType)) {
                console.log(`Applying default style for unmatched node type: ${nodeType}`);
                enhancedStyles.push({
                    selector: `node[type="${nodeType}"]`,
                    style: {
                        'background-color': '#808080',
                        'shape': 'ellipse',
                        'width': '30px',
                        'height': '30px'
                    }
                });
            }
        });

        // Find all actual edge types in the graph and apply default styles for unmatched ones
        const actualEdgeTypes = new Set();
        edges.forEach(edge => {
            const edgeType = edge.data('type');
            if (edgeType) {
                actualEdgeTypes.add(edgeType);
            }
        });

        // Apply default styles for unmatched edge types
        actualEdgeTypes.forEach(edgeType => {
            if (!edgeTypes.includes(edgeType)) {
                console.log(`Applying default style for unmatched edge type: ${edgeType}`);
                enhancedStyles.push({
                    selector: `edge[type="${edgeType}"]`,
                    style: {
                        'line-color': '#CCCCCC',
                        'target-arrow-color': '#CCCCCC',
                        'target-arrow-shape': 'triangle',
                        'width': 1,
                        'line-style': 'solid'
                    }
                });
            }
        });

        // Selection style
        enhancedStyles.push({
            selector: 'node:selected',
            style: {
                'border-color': '#ff4757',
                'border-width': '3px'
            }
        });

        console.log('Enhanced styles to apply:', enhancedStyles);
        
        try {
            // Clear existing styles first
            this.cy.style().resetToDefault();
            
            // Apply new styles
            this.cy.style(enhancedStyles);
            console.log('Enhanced styles applied successfully');
            
            // Force a refresh of the styles
            this.cy.style().update();
            
            // Force a redraw
            this.cy.forceRender();
            
            // Trigger a layout refresh
            setTimeout(() => {
                this.cy.fit();
                this.cy.center();
            }, 100);
            
        } catch (error) {
            console.error('Error applying enhanced styles:', error);
        }
    }

    applyFilters(selectedNodeTypes, selectedEdgeTypes) {
        if (!this.cy || this.originalNodes.length === 0) return;

        try {
            // Instead of removing and re-adding elements, just show/hide them
            // This preserves the current positions
            
            // Handle nodes
            this.cy.nodes().forEach(node => {
                const nodeType = node.data('type');
                const shouldShow = selectedNodeTypes.size === 0 || selectedNodeTypes.has(nodeType);
                
                if (shouldShow) {
                    node.style('display', 'element'); // Show the node
                } else {
                    node.style('display', 'none'); // Hide the node
                }
            });

            // Handle edges
            this.cy.edges().forEach(edge => {
                const edgeType = edge.data('type');
                const sourceNode = this.cy.getElementById(edge.data('source'));
                const targetNode = this.cy.getElementById(edge.data('target'));
                
                // Edge should be visible if:
                // 1. Its type is selected (or no edge types are filtered)
                // 2. Both source and target nodes are visible
                const typeMatch = selectedEdgeTypes.size === 0 || selectedEdgeTypes.has(edgeType);
                const sourceVisible = sourceNode.style('display') !== 'none';
                const targetVisible = targetNode.style('display') !== 'none';
                const shouldShow = typeMatch && sourceVisible && targetVisible;
                
                if (shouldShow) {
                    edge.style('display', 'element'); // Show the edge
                } else {
                    edge.style('display', 'none'); // Hide the edge
                }
            });

            // Count visible elements for logging
            const visibleNodes = this.cy.nodes().filter(node => node.style('display') !== 'none');
            const visibleEdges = this.cy.edges().filter(edge => edge.style('display') !== 'none');
            
            console.log(`Applied filters: ${visibleNodes.length} visible nodes, ${visibleEdges.length} visible edges (positions preserved)`);
        } catch (error) {
            console.error('Error applying filters:', error);
        }
    }

    resetElementVisibility() {
        if (!this.cy) return;
        
        // Show all nodes and edges
        this.cy.elements().style('display', 'element');
        console.log('Reset all element visibility - all nodes and edges are now visible');
    }

    applyLayout(layoutName) {
        if (!this.cy) return;

        // Get only visible elements for layout calculation
        const visibleNodes = this.cy.nodes().filter(node => node.style('display') !== 'none');
        const visibleEdges = this.cy.edges().filter(edge => edge.style('display') !== 'none');
        
        console.log(`Applying ${layoutName} layout to ${visibleNodes.length} visible nodes and ${visibleEdges.length} visible edges`);

        const layoutConfig = {
            name: layoutName,
            animate: true,
            animationDuration: 500,
            fit: true,
            padding: 30
        };

        // Add layout-specific configurations with dynamic sizing based on visible elements
        switch (layoutName) {
            case 'cose':
                layoutConfig.nodeRepulsion = 400000;
                layoutConfig.idealEdgeLength = 100;
                layoutConfig.edgeElasticity = 100;
                break;
            case 'cose-bilkent':
                if (this.coseBilkentAvailable) {
                    layoutConfig.nodeRepulsion = 4500;
                    layoutConfig.idealEdgeLength = 50;
                    layoutConfig.edgeElasticity = 0.45;
                    layoutConfig.nestingFactor = 0.1;
                } else {
                    console.warn('Cose-bilkent layout not available, falling back to cose');
                    layoutConfig.name = 'cose';
                }
                break;
            case 'grid':
                // Calculate grid based on visible nodes only
                layoutConfig.rows = Math.ceil(Math.sqrt(visibleNodes.length));
                break;
            case 'circle':
                // Calculate radius based on visible nodes
                const nodeCount = visibleNodes.length;
                layoutConfig.radius = Math.max(100, Math.min(300, nodeCount * 30));
                break;
            case 'concentric':
                layoutConfig.concentric = function(node) {
                    return node.degree();
                };
                layoutConfig.levelWidth = function(nodes) {
                    return 2;
                };
                break;
        }

        try {
            // Apply layout only to visible elements
            const visibleElements = visibleNodes.union(visibleEdges);
            const layout = visibleElements.layout(layoutConfig);
            layout.run();
        } catch (error) {
            console.error(`Error applying ${layoutName} layout:`, error);
            // Fallback to basic cose layout on visible elements only
            const visibleElements = visibleNodes.union(visibleEdges);
            visibleElements.layout({ name: 'cose', animate: true }).run();
        }
    }

    updateGraph(validNodes, validEdges) {
        if (!this.isInitialized || !this.cy) {
            console.warn('Cytoscape not initialized yet, cannot update graph');
            return false;
        }

        try {
            // Store original data
            this.originalNodes = [...validNodes];
            this.originalEdges = [...validEdges];

            // Clear existing elements
            this.cy.elements().remove();
            
            // Add new elements with error handling
            this.cy.add(validNodes);
            console.log(`Added ${validNodes.length} nodes to graph`);
            
            this.cy.add(validEdges);
            console.log(`Added ${validEdges.length} edges to graph`);
            
            // Apply layout
            this.applyLayout('cose');
            
            // Apply current view mode
            this.applyViewMode();
            
            // Fit the graph to view after layout completes
            setTimeout(() => {
                this.cy.fit();
            }, 600);
            
            console.log(`Graph updated successfully: ${validNodes.length} nodes, ${validEdges.length} edges`);
            return true;
            
        } catch (error) {
            console.error('Error updating graph:', error);
            this.showError('Error rendering graph elements. Please check the console for details.');
            return false;
        }
    }

    fit() {
        if (this.cy) {
            this.cy.fit();
        }
    }

    showError(message) {
        const container = document.getElementById('cy');
        if (container) {
            // Use sanitize function if available, otherwise create safe element manually
            if (typeof sanitize === 'function') {
                container.innerHTML = `<div style="display: flex; align-items: center; justify-content: center; height: 100%; color: #ff6b6b; font-family: Arial, sans-serif; text-align: center; padding: 20px;">${sanitize(message)}</div>`;
            } else {
                // Fallback: create element safely without innerHTML
                const errorDiv = document.createElement('div');
                errorDiv.style.cssText = 'display: flex; align-items: center; justify-content: center; height: 100%; color: #ff6b6b; font-family: Arial, sans-serif; text-align: center; padding: 20px;';
                errorDiv.textContent = message;
                container.innerHTML = '';
                container.appendChild(errorDiv);
            }
        }
    }

    getCoseBilkentAvailable() {
        return this.coseBilkentAvailable;
    }

    getOriginalData() {
        return {
            nodes: this.originalNodes,
            edges: this.originalEdges
        };
    }

    setEventHandlers(onNodeSelected, onEdgeSelected, onSelectionCleared) {
        this.onNodeSelected = onNodeSelected;
        this.onEdgeSelected = onEdgeSelected;
        this.onSelectionCleared = onSelectionCleared;
    }

    getIsInitialized() {
        return this.isInitialized;
    }
}
