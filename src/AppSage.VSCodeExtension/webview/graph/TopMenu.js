/**
 * TopMenu - Handles all top menu controls including layout selection, filters, and view modes
 */
class TopMenu {
    constructor(graphRenderer, sidePanel) {
        console.log('=== TOPMENU CONSTRUCTOR START ===');
        this.graphRenderer = graphRenderer;
        this.sidePanel = sidePanel;
        console.log('TopMenu constructor - sidePanel received:', !!sidePanel);
        console.log('TopMenu constructor - graphRenderer received:', !!graphRenderer);
        
        this.selectedNodeTypes = new Set();
        this.selectedEdgeTypes = new Set();
        this.allNodeTypes = new Set();
        this.allEdgeTypes = new Set();
        this.currentViewMode = 'basic';
        this.coseBilkentAvailable = false;
        
        this.initialize();
        console.log('=== TOPMENU CONSTRUCTOR END ===');
    }

    initialize() {
        this.setupControls();
        this.setupFilterControls();
        this.setupViewControls();
    }

    setupControls() {
        const fitBtn = document.getElementById('fitBtn');
        const resetBtn = document.getElementById('resetBtn');
        const layoutSelect = document.getElementById('layoutSelect');

        if (fitBtn) {
            fitBtn.addEventListener('click', () => {
                this.graphRenderer.fit();
            });
        }

        if (resetBtn) {
            resetBtn.addEventListener('click', () => {
                // Reset filters to show all types
                this.selectedNodeTypes = new Set(this.allNodeTypes);
                this.selectedEdgeTypes = new Set(this.allEdgeTypes);
                this.populateFilterDropdowns();
                this.applyFilters();
                
                // Fit to view
                setTimeout(() => {
                    this.graphRenderer.fit();
                }, 100);
            });
        }

        if (layoutSelect) {
            layoutSelect.addEventListener('change', (e) => {
                const layoutName = e.target.value;
                this.graphRenderer.applyLayout(layoutName);
            });
        }
    }

    setupFilterControls() {
        const nodeFilterBtn = document.getElementById('nodeFilterBtn');
        const edgeFilterBtn = document.getElementById('edgeFilterBtn');
        const nodeFilterDropdown = document.getElementById('nodeFilterDropdown');
        const edgeFilterDropdown = document.getElementById('edgeFilterDropdown');

        // Toggle node filter dropdown
        if (nodeFilterBtn) {
            nodeFilterBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                nodeFilterDropdown.classList.toggle('show');
                edgeFilterDropdown.classList.remove('show');
            });
        }

        // Toggle edge filter dropdown
        if (edgeFilterBtn) {
            edgeFilterBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                edgeFilterDropdown.classList.toggle('show');
                nodeFilterDropdown.classList.remove('show');
            });
        }

        // Close dropdowns when clicking outside
        document.addEventListener('click', () => {
            nodeFilterDropdown.classList.remove('show');
            edgeFilterDropdown.classList.remove('show');
            // Close view dropdown too
            const viewFilterDropdown = document.getElementById('viewFilterDropdown');
            if (viewFilterDropdown) {
                viewFilterDropdown.classList.remove('show');
            }
        });

        // Setup select all checkboxes
        const selectAllNodes = document.getElementById('selectAllNodes');
        const selectAllEdges = document.getElementById('selectAllEdges');

        if (selectAllNodes) {
            selectAllNodes.addEventListener('change', (e) => {
                const checked = e.target.checked;
                const checkboxes = document.querySelectorAll('#nodeFilterList input[type="checkbox"]');
                checkboxes.forEach(cb => {
                    cb.checked = checked;
                    const nodeType = cb.value;
                    if (checked) {
                        this.selectedNodeTypes.add(nodeType);
                    } else {
                        this.selectedNodeTypes.delete(nodeType);
                    }
                });
                this.applyFilters();
            });
        }

        if (selectAllEdges) {
            selectAllEdges.addEventListener('change', (e) => {
                const checked = e.target.checked;
                const checkboxes = document.querySelectorAll('#edgeFilterList input[type="checkbox"]');
                checkboxes.forEach(cb => {
                    cb.checked = checked;
                    const edgeType = cb.value;
                    if (checked) {
                        this.selectedEdgeTypes.add(edgeType);
                    } else {
                        this.selectedEdgeTypes.delete(edgeType);
                    }
                });
                this.applyFilters();
            });
        }
    }

    setupViewControls() {
        console.log('=== TOPMENU SETUPVIEWCONTROLS START ===');
        const viewFilterBtn = document.getElementById('viewFilterBtn');
        const viewFilterDropdown = document.getElementById('viewFilterDropdown');
        const showSidePanelCheckbox = document.getElementById('showSidePanel');
        const basicViewRadio = document.getElementById('basicView');
        const enhancedViewRadio = document.getElementById('enhancedView');
        const customizeEnhancedViewBtn = document.getElementById('customizeEnhancedView');

        console.log('setupViewControls - showSidePanelCheckbox found:', !!showSidePanelCheckbox);
        console.log('setupViewControls - sidePanel available:', !!this.sidePanel);
        
        if (showSidePanelCheckbox) {
            console.log('setupViewControls - showSidePanelCheckbox current checked state:', showSidePanelCheckbox.checked);
        }

        // Set Show Side Panel as checked by default and open sidebar
        if (showSidePanelCheckbox) {
            showSidePanelCheckbox.checked = true;
            console.log('setupViewControls - showSidePanelCheckbox set to checked');
            // Open the sidebar by default since Show Side Panel is checked - use small delay to ensure DOM is ready
            setTimeout(() => {
                console.log('setupViewControls - calling showInitially() after timeout');
                this.sidePanel.showInitially();
            }, 100);
        } else {
            console.error('setupViewControls - showSidePanelCheckbox not found!');
        }

        // Toggle view filter dropdown
        if (viewFilterBtn) {
            viewFilterBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                viewFilterDropdown.classList.toggle('show');
                // Close other dropdowns
                const nodeFilterDropdown = document.getElementById('nodeFilterDropdown');
                const edgeFilterDropdown = document.getElementById('edgeFilterDropdown');
                if (nodeFilterDropdown) nodeFilterDropdown.classList.remove('show');
                if (edgeFilterDropdown) edgeFilterDropdown.classList.remove('show');
            });
        }

        // Handle Show Side Panel checkbox
        if (showSidePanelCheckbox) {
            showSidePanelCheckbox.addEventListener('change', (e) => {
                console.log('TopMenu: Show Side Panel checkbox changed, checked =', e.target.checked);
                if (e.target.checked) {
                    console.log('Show Side Panel enabled');
                    this.sidePanel.toggle(true);
                } else {
                    console.log('Show Side Panel disabled');
                    this.sidePanel.toggle(false);
                }
            });
        }

        // Handle view mode changes
        if (basicViewRadio) {
            basicViewRadio.addEventListener('change', (e) => {
                if (e.target.checked) {
                    console.log('Basic view selected');
                    this.currentViewMode = 'basic';
                    this.graphRenderer.setViewMode('basic');
                }
            });
        }

        if (enhancedViewRadio) {
            enhancedViewRadio.addEventListener('change', (e) => {
                if (e.target.checked) {
                    console.log('Enhanced view selected');
                    this.currentViewMode = 'enhanced';
                    this.graphRenderer.setViewMode('enhanced');
                }
            });
        }

        // Handle customize enhanced view button
        if (customizeEnhancedViewBtn) {
            customizeEnhancedViewBtn.addEventListener('click', (e) => {
                e.preventDefault();
                console.log('Customize Enhanced View button clicked');
                
                this.sidePanel.showCustomizeTab();
                
                // Close the dropdown
                viewFilterDropdown.classList.remove('show');
            });
        }
    }

    updateTypes(nodes, edges) {
        // Clear existing types
        this.allNodeTypes.clear();
        this.allEdgeTypes.clear();
        
        // Collect all node and edge types
        nodes.forEach(node => {
            this.allNodeTypes.add(node.data.type);
        });
        
        edges.forEach(edge => {
            this.allEdgeTypes.add(edge.data.type);
        });

        // Initialize filters to include all types if not already set
        if (this.selectedNodeTypes.size === 0) {
            this.selectedNodeTypes = new Set(this.allNodeTypes);
        }
        if (this.selectedEdgeTypes.size === 0) {
            this.selectedEdgeTypes = new Set(this.allEdgeTypes);
        }

        // Update filter dropdowns
        this.populateFilterDropdowns();
    }

    populateFilterDropdowns() {
        this.populateNodeFilters();
        this.populateEdgeFilters();
    }

    populateNodeFilters() {
        const nodeFilterList = document.getElementById('nodeFilterList');
        if (!nodeFilterList) return;

        nodeFilterList.innerHTML = '';
        const sortedNodeTypes = Array.from(this.allNodeTypes).sort();

        sortedNodeTypes.forEach(nodeType => {
            const filterItem = document.createElement('div');
            filterItem.className = 'filter-item';
            
            const label = document.createElement('label');
            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.value = nodeType;
            checkbox.checked = this.selectedNodeTypes.has(nodeType);
            
            checkbox.addEventListener('change', (e) => {
                if (e.target.checked) {
                    this.selectedNodeTypes.add(nodeType);
                } else {
                    this.selectedNodeTypes.delete(nodeType);
                }
                this.updateSelectAllNodes();
                this.applyFilters();
            });
            
            label.appendChild(checkbox);
            label.appendChild(document.createTextNode(nodeType));
            filterItem.appendChild(label);
            nodeFilterList.appendChild(filterItem);
        });

        this.updateSelectAllNodes();
    }

    populateEdgeFilters() {
        const edgeFilterList = document.getElementById('edgeFilterList');
        if (!edgeFilterList) return;

        edgeFilterList.innerHTML = '';
        const sortedEdgeTypes = Array.from(this.allEdgeTypes).sort();

        sortedEdgeTypes.forEach(edgeType => {
            const filterItem = document.createElement('div');
            filterItem.className = 'filter-item';
            
            const label = document.createElement('label');
            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.value = edgeType;
            checkbox.checked = this.selectedEdgeTypes.has(edgeType);
            
            checkbox.addEventListener('change', (e) => {
                if (e.target.checked) {
                    this.selectedEdgeTypes.add(edgeType);
                } else {
                    this.selectedEdgeTypes.delete(edgeType);
                }
                this.updateSelectAllEdges();
                this.applyFilters();
            });
            
            label.appendChild(checkbox);
            label.appendChild(document.createTextNode(edgeType));
            filterItem.appendChild(label);
            edgeFilterList.appendChild(filterItem);
        });

        this.updateSelectAllEdges();
    }

    updateSelectAllNodes() {
        const selectAllNodes = document.getElementById('selectAllNodes');
        if (!selectAllNodes) return;

        const allSelected = this.allNodeTypes.size > 0 && this.selectedNodeTypes.size === this.allNodeTypes.size;
        const noneSelected = this.selectedNodeTypes.size === 0;
        
        selectAllNodes.checked = allSelected;
        selectAllNodes.indeterminate = !allSelected && !noneSelected;
    }

    updateSelectAllEdges() {
        const selectAllEdges = document.getElementById('selectAllEdges');
        if (!selectAllEdges) return;

        const allSelected = this.allEdgeTypes.size > 0 && this.selectedEdgeTypes.size === this.allEdgeTypes.size;
        const noneSelected = this.selectedEdgeTypes.size === 0;
        
        selectAllEdges.checked = allSelected;
        selectAllEdges.indeterminate = !allSelected && !noneSelected;
    }

    applyFilters() {
        this.graphRenderer.applyFilters(this.selectedNodeTypes, this.selectedEdgeTypes);
    }

    setCoseBilkentAvailable(available) {
        this.coseBilkentAvailable = available;
    }

    getCurrentViewMode() {
        return this.currentViewMode;
    }

    getSelectedTypes() {
        return {
            nodeTypes: this.selectedNodeTypes,
            edgeTypes: this.selectedEdgeTypes
        };
    }

    getAllTypes() {
        return {
            nodeTypes: this.allNodeTypes,
            edgeTypes: this.allEdgeTypes
        };
    }
}
