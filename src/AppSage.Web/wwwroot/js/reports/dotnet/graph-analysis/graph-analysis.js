// Graph Analysis JavaScript Module
// This script handles the Cytoscape graph visualization for dependency analysis

class GraphAnalysis {
    constructor(config) {
        this.cy = null;
        this.graphData = config.graphData;
        this.edgeDistances = config.edgeDistances;
        this.groupingConfigs = config.groupingConfigs;
        this.nodeLegend = config.nodeLegend;
        this.edgeLegend = config.edgeLegend;
        
        this.init();
    }

    init() {
        console.log('Graph data:', this.graphData);
        console.log('Edge distances:', this.edgeDistances);
        console.log('Grouping configs:', this.groupingConfigs);

        // Check if we have valid data
        if (this.graphData && this.graphData.nodes) {
            console.log(`Found ${this.graphData.nodes.length} nodes and ${this.graphData.edges ? this.graphData.edges.length : 0} edges`);
            
            // Debug: Log first few nodes to understand data structure
            if (this.graphData.nodes.length > 0) {
                console.log('Sample node data structure:');
                for (let i = 0; i < Math.min(3, this.graphData.nodes.length); i++) {
                    console.log(`Node ${i}:`, this.graphData.nodes[i]);
                }
            }
            
            if (this.graphData.edges && this.graphData.edges.length > 0) {
                console.log('Sample edge data structure:');
                for (let i = 0; i < Math.min(3, this.graphData.edges.length); i++) {
                    console.log(`Edge ${i}:`, this.graphData.edges[i]);
                }
            }
        } else {
            console.warn('Graph data is missing or invalid');
        }

        this.initializeGraph();
    }

    // Initialize Cytoscape
    initializeGraph() {
        // Validate data before proceeding
        if (!this.graphData || !this.graphData.nodes || !this.graphData.edges) {
            console.error('Invalid graph data:', this.graphData);
            document.getElementById('graph-container').innerHTML = '<div class="alert alert-warning text-center">No graph data available. Please check that data has been loaded for this analysis.</div>';
            return;
        }

        if (this.graphData.nodes.length === 0) {
            console.warn('No nodes in graph data');
            document.getElementById('graph-container').innerHTML = '<div class="alert alert-info text-center">No nodes found in the dependency graph. This might indicate no dependencies were analyzed or no data matches the current filters.</div>';
            return;
        }

        const elements = [
            ...this.graphData.nodes.map(node => ({
                group: 'nodes',
                data: {
                    id: node.data.id,
                    label: node.data.label,
                    type: node.data.type,
                    attributes: node.data.attributes,
                    parent: node.data.parent
                }
            })),
            ...this.graphData.edges.map(edge => ({
                group: 'edges',
                data: {
                    id: edge.data.id,
                    source: edge.data.source,
                    target: edge.data.target,
                    label: edge.data.label,
                    type: edge.data.type,
                    attributes: edge.data.attributes
                }
            }))
        ];

        console.log('Cytoscape elements:', elements);

        try {
            this.cy = cytoscape({
                container: document.getElementById('graph-container'),
                elements: elements,
                style: [
                    {
                        selector: 'node',
                        style: {
                            'background-color': (node) => this.getNodeColor(node.data('type')),
                            'label': 'data(label)',
                            'font-size': '10px',
                            'text-valign': 'center',
                            'text-halign': 'center',
                            'color': '#333',
                            'text-outline-width': 1,
                            'text-outline-color': '#fff',
                            'width': (node) => this.getNodeSize(node.data('attributes')),
                            'height': (node) => this.getNodeSize(node.data('attributes')),
                            'shape': (node) => this.getNodeShape(node.data('type')),
                            'border-width': 2,
                            'border-color': '#fff',
                            'overlay-padding': '6px',
                            'z-index': 10
                        }
                    },
                    {
                        selector: 'node[type="group"]',
                        style: {
                            'background-color': '#f0f0f0',
                            'background-opacity': 0.3,
                            'border-width': 2,
                            'border-color': '#999',
                            'border-opacity': 0.7,
                            'shape': 'roundrectangle',
                            'padding': '20px',
                            'text-valign': 'top',
                            'text-halign': 'center',
                            'font-size': '12px',
                            'font-weight': 'bold',
                            'color': '#666',
                            'z-index': 1
                        }
                    },
                    {
                        selector: 'node:parent',
                        style: {
                            'background-opacity': 0.2,
                            'border-width': 2,
                            'border-color': '#666',
                            'border-style': 'dashed',
                            'text-valign': 'top',
                            'text-halign': 'center',
                            'font-size': '12px',
                            'font-weight': 'bold',
                            'color': '#666',
                            'z-index': 1
                        }
                    },
                    {
                        selector: 'node:selected',
                        style: {
                            'border-width': 4,
                            'border-color': '#FD5A4A',
                            'background-color': '#FD5A4A'
                        }
                    },
                    {
                        selector: 'edge',
                        style: {
                            'curve-style': 'bezier',
                            'opacity': 0.8,
                            'width': (edge) => this.getEdgeWidth(edge.data('type')),
                            'line-color': (edge) => this.getEdgeColor(edge.data('type')),
                            'target-arrow-color': (edge) => this.getEdgeColor(edge.data('type')),
                            'target-arrow-shape': (edge) => this.getEdgeArrowShape(edge.data('type')),
                            'line-style': (edge) => this.getEdgeLineStyle(edge.data('type')),
                            'z-index': 5,
                            'overlay-padding': '3px'
                        }
                    },
                    {
                        selector: 'edge:selected',
                        style: {
                            'line-color': '#FD5A4A',
                            'target-arrow-color': '#FD5A4A',
                            'width': 4,
                            'opacity': 1
                        }
                    },
                    {
                        selector: '.filtered',
                        style: {
                            'opacity': 0.2,
                            'z-index': 1
                        }
                    },
                    {
                        selector: '.highlighted',
                        style: {
                            'opacity': 1,
                            'z-index': 999
                        }
                    }
                ],
                layout: {
                    name: 'cose',
                    animate: true,
                    animationDuration: 1000,
                    nodeRepulsion: 10000,
                    nodeOverlap: 5,
                    idealEdgeLength: (edge) => {
                        const edgeType = edge.data('type') || 'No Type';
                        return this.edgeDistances[edgeType] || 50;
                    },
                    edgeElasticity: (edge) => {
                        const edgeType = edge.data('type') || 'No Type';
                        return 100 / (this.edgeDistances[edgeType] || 50);
                    }
                },
                zoom: 1,
                pan: { x: 0, y: 0 },
                minZoom: 0.1,
                maxZoom: 3,
                wheelSensitivity: 0.1
            });

            console.log('Cytoscape initialized successfully:', this.cy);

            // Add event listeners
            this.setupEventListeners();
            this.updateVisibleCounts();
        } catch (error) {
            console.error('Error initializing Cytoscape:', error);
            document.getElementById('graph-container').innerHTML = '<div class="alert alert-danger text-center">Error initializing graph visualization. Please check the browser console for more details.</div>';
        }
    }

    // Helper functions for styling
    getNodeColor(nodeType) {
        const colors = {
            'Project': '#1976d2',
            'Class': '#388e3c',
            'Method': '#f57c00',
            'Assembly': '#7b1fa2',
            'Namespace': '#00796b',
            'Solution': '#d32f2f'
        };
        return colors[nodeType] || '#666666';
    }

    getNodeShape(nodeType) {
        const shapes = {
            'Project': 'rectangle',
            'Class': 'ellipse',
            'Method': 'triangle',
            'Assembly': 'hexagon',
            'Namespace': 'roundrectangle',
            'Solution': 'diamond'
        };
        return shapes[nodeType] || 'ellipse';
    }

    getNodeSize(attributes) {
        if (attributes && attributes['LinesOfCode']) {
            const loc = parseInt(attributes['LinesOfCode']);
            return Math.max(20, Math.min(80, 20 + (loc / 50)));
        }
        return 30;
    }

    getEdgeColor(edgeType) {
        const colors = {
            'Inherit': '#d32f2f',
            'Implement': '#1976d2',
            'Use': '#388e3c',
            'Invoke': '#f57c00',
            'Access': '#7b1fa2',
            'Create': '#00796b',
            'Refer': '#757575'
        };
        return colors[edgeType] || '#cccccc';
    }

    getEdgeWidth(edgeType) {
        const widths = {
            'Inherit': 4,
            'Implement': 3
        };
        return widths[edgeType] || 2;
    }

    getEdgeLineStyle(edgeType) {
        return edgeType === 'Refer' ? 'dashed' : 'solid';
    }

    getEdgeArrowShape(edgeType) {
        const shapes = {
            'Inherit': 'triangle-backcurve',
            'Implement': 'triangle-cross'
        };
        return shapes[edgeType] || 'triangle';
    }

    // Setup event listeners
    setupEventListeners() {
        // Node/Edge hover effects
        this.cy.on('mouseover', 'node', (evt) => {
            const node = evt.target;
            const neighborhood = node.neighborhood().add(node);
            
            this.cy.elements().addClass('filtered');
            neighborhood.removeClass('filtered').addClass('highlighted');
        });

        this.cy.on('mouseout', 'node', (evt) => {
            this.cy.elements().removeClass('filtered highlighted');
        });

        // Node click for details
        this.cy.on('tap', 'node', (evt) => {
            const node = evt.target;
            this.showNodeDetails(node);
        });

        // Graph controls
        document.getElementById('btn-fit').addEventListener('click', () => this.cy.fit());
        document.getElementById('btn-center').addEventListener('click', () => this.cy.center());
        document.getElementById('btn-layout').addEventListener('click', () => this.runLayout());
        document.getElementById('btn-export').addEventListener('click', () => this.exportGraph());

        // Layout controls
        document.querySelectorAll('input[name="layout"]').forEach(radio => {
            radio.addEventListener('change', () => {
                if (radio.checked) {
                    this.runLayout(radio.id.replace('layout-', ''));
                }
            });
        });

        // Filter controls - updated for checkbox-based filtering
        document.querySelectorAll('.node-filter-checkbox, .edge-filter-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', () => {
                console.log('Filter changed:', {
                    filterType: checkbox.classList.contains('node-filter-checkbox') ? 'node' : 'edge',
                    nodeType: checkbox.dataset.nodeType,
                    edgeType: checkbox.dataset.edgeType,
                    checked: checkbox.checked
                });
                
                this.applyFilters();
            });
        });

        // Distance controls
        document.querySelectorAll('.distance-slider').forEach(slider => {
            slider.addEventListener('input', () => {
                const edgeType = slider.dataset.edgeType;
                const value = parseInt(slider.value);
                this.edgeDistances[edgeType] = value;
                slider.nextElementSibling.textContent = value;
                this.runLayout();
            });
        });

        // Grouping controls
        document.querySelectorAll('.grouping-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', () => {
                const groupingKey = checkbox.dataset.groupingKey;
                this.groupingConfigs[groupingKey].enabled = checkbox.checked;
                this.applyGrouping();
            });
        });
    }

    clearAllFilters() {
        console.log('Clearing all filters');
        
        // Check all filter checkboxes (both node and edge)
        document.querySelectorAll('.node-filter-checkbox, .edge-filter-checkbox').forEach(checkbox => {
            checkbox.checked = true;
        });
        
        // Apply filters (should show all nodes and edges again)
        this.applyFilters();
    }

    // Apply filters
    applyFilters() {
        console.log("=== Applying filters ===");
        
        // Collect enabled node types from checkboxes
        const enabledNodeTypes = [];
        document.querySelectorAll('.node-filter-checkbox:checked').forEach(checkbox => {
            enabledNodeTypes.push(checkbox.dataset.nodeType);
        });

        // Collect enabled edge types from checkboxes
        const enabledEdgeTypes = [];
        document.querySelectorAll('.edge-filter-checkbox:checked').forEach(checkbox => {
            enabledEdgeTypes.push(checkbox.dataset.edgeType);
        });

        console.log("Enabled node types:", enabledNodeTypes);
        console.log("Enabled edge types:", enabledEdgeTypes);

        // Apply node filters
        let totalNodes = 0;
        let visibleNodes = 0;
        
        this.cy.nodes().forEach(node => {
            totalNodes++;
            const nodeData = node.data();
            const nodeType = nodeData.type || 'No Type';
            
            // Show node if its type is enabled (or if no types are selected, show all)
            const visible = enabledNodeTypes.length === 0 || enabledNodeTypes.includes(nodeType);
            
            if (visible) {
                visibleNodes++;
                node.style('display', 'element');
            } else {
                node.style('display', 'none');
            }
        });

        console.log(`Node filtering result: ${visibleNodes}/${totalNodes} nodes visible`);

        // Apply edge filters
        let totalEdges = 0;
        let visibleEdges = 0;
        
        this.cy.edges().forEach(edge => {
            totalEdges++;
            const edgeData = edge.data();
            const edgeType = edgeData.type || 'No Type';
            
            // First check if both endpoints are visible
            const sourceVisible = edge.source().style('display') !== 'none';
            const targetVisible = edge.target().style('display') !== 'none';
            
            if (!sourceVisible || !targetVisible) {
                edge.style('display', 'none');
                return;
            }
            
            // Show edge if its type is enabled (or if no types are selected, show all)
            const visible = enabledEdgeTypes.length === 0 || enabledEdgeTypes.includes(edgeType);
            
            if (visible) {
                visibleEdges++;
                edge.style('display', 'element');
            } else {
                edge.style('display', 'none');
            }
        });

        console.log(`Edge filtering result: ${visibleEdges}/${totalEdges} edges visible`);
        
        this.updateVisibleCounts();
        console.log("=== Filters applied successfully ===");
    }

    // Apply grouping
    applyGrouping() {
        console.log('=== Applying grouping ===');
        console.log('Grouping configs:', this.groupingConfigs);
        
        // Remove existing parent relationships
        this.cy.nodes().forEach(node => {
            node.move({ parent: null });
        });

        // Remove existing compound nodes
        this.cy.nodes('[id ^= "group-"]').remove();

        // Apply enabled groupings
        for (const [key, config] of Object.entries(this.groupingConfigs)) {
            if (config.enabled) {
                console.log(`Applying grouping: ${config.name}`);
                const groups = {};
                
                // Find anchor nodes (nodes that define groups)
                this.cy.nodes().forEach(node => {
                    const nodeData = node.data();
                    // Check if this is an anchor node (Project, Assembly, or Namespace)
                    if (nodeData.type === config.groupByValue) {
                        console.log(`Found anchor node: ${nodeData.label} (${nodeData.type})`);
                        
                        // Find all nodes connected to this anchor node
                        const connectedNodes = node.neighborhood('node');
                        const groupId = `group-${key}-${node.id()}`;
                        
                        if (connectedNodes.length > 0) {
                            groups[groupId] = {
                                anchorNode: node,
                                connectedNodes: connectedNodes,
                                config: config
                            };
                            console.log(`Group ${groupId} has ${connectedNodes.length} connected nodes`);
                        }
                    }
                });

                // Create compound nodes for each group
                for (const [groupId, groupData] of Object.entries(groups)) {
                    if (groupData.connectedNodes.length > 0) {
                        console.log(`Creating compound node: ${groupId}`);
                        
                        // Add compound node
                        this.cy.add({
                            group: 'nodes',
                            data: { 
                                id: groupId, 
                                label: `${groupData.anchorNode.data('label')} Group`,
                                type: 'group'
                            },
                            style: {
                                'background-color': config.color,
                                'border-width': 2,
                                'border-color': config.borderColor,
                                'shape': 'roundrectangle',
                                'padding': '20px',
                                'text-valign': 'top',
                                'text-halign': 'center',
                                'font-size': '12px',
                                'font-weight': 'bold'
                            }
                        });

                        // Move connected nodes to the group
                        groupData.connectedNodes.forEach(node => {
                            console.log(`Moving node ${node.data('label')} to group ${groupId}`);
                            node.move({ parent: groupId });
                        });
                        
                        // Also move the anchor node to the group
                        groupData.anchorNode.move({ parent: groupId });
                    }
                }
            }
        }

        console.log('=== Grouping applied, running layout ===');
        this.runLayout();
    }

    // Run layout
    runLayout(layoutName = 'cose') {
        if (!this.cy) return;
        
        const layoutOptions = {
            name: layoutName,
            animate: true,
            animationDuration: 1000
        };

        // Add specific options based on layout type
        if (layoutName === 'cose') {
            layoutOptions.nodeRepulsion = 10000;
            layoutOptions.nodeOverlap = 10;
            layoutOptions.idealEdgeLength = (edge) => {
                const edgeType = edge.data('type') || 'No Type';
                return this.edgeDistances[edgeType] || 50;
            };
            layoutOptions.edgeElasticity = (edge) => {
                const edgeType = edge.data('type') || 'No Type';
                return 100 / (this.edgeDistances[edgeType] || 50);
            };
            // Better handling of compound nodes
            layoutOptions.nodeDimensionsIncludeLabels = true;
            layoutOptions.padding = 20;
            layoutOptions.componentSpacing = 100;
        } else if (layoutName === 'circle') {
            layoutOptions.radius = 200;
        } else if (layoutName === 'grid') {
            layoutOptions.rows = Math.ceil(Math.sqrt(this.cy.nodes().length));
        } else if (layoutName === 'dagre') {
            layoutOptions.rankDir = 'TB';
            layoutOptions.nodeSep = 50;
            layoutOptions.rankSep = 100;
        }

        const layout = this.cy.layout(layoutOptions);
        layout.run();
    }

    // Export graph
    exportGraph() {
        if (!this.cy) return;
        
        const png64 = this.cy.png({
            output: 'base64uri',
            full: true,
            scale: 2
        });
        
        const link = document.createElement('a');
        link.href = png64;
        link.download = `graph-analysis-${new Date().toISOString().slice(0, 10)}.png`;
        link.click();
    }

    // Show node details
    showNodeDetails(node) {
        const data = node.data();
        let details = `<strong>${data.label}</strong><br>`;
        details += `Type: ${data.type}<br>`;
        
        for (const [key, value] of Object.entries(data.attributes || {})) {
            details += `${key}: ${value}<br>`;
        }

        console.log(details);
        // You could show this in a modal or sidebar in the future
    }

    // Update visible counts
    updateVisibleCounts() {
        if (!this.cy) return;
        
        const visibleNodes = this.cy.nodes('[style != "display: none"]').length;
        const visibleEdges = this.cy.edges('[style != "display: none"]').length;
        
        document.getElementById('visible-nodes').textContent = visibleNodes;
        document.getElementById('visible-edges').textContent = visibleEdges;
    }
}

// Global initialization function
window.initializeGraphAnalysis = function(config) {
    return new GraphAnalysis(config);
};
