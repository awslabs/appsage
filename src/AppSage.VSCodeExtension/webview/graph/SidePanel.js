/**
 * SidePanel - Handles sidebar panel functionality including tabs, properties display, and legend
 */
class SidePanel {
    constructor(vscode, enhancedViewCustomizer) {
        console.log('=== SIDEPANEL CONSTRUCTOR START ===');
        this.vscode = vscode;
        this.enhancedViewCustomizer = enhancedViewCustomizer;
        this.sidebarOpen = true; // Default to true since Show Side Panel is checked by default
        this.activeTab = 'properties';
        
        console.log('SidePanel constructor - sidebarOpen set to:', this.sidebarOpen);
        console.log('SidePanel constructor - activeTab set to:', this.activeTab);
        
        this.initialize();
        console.log('=== SIDEPANEL CONSTRUCTOR END ===');
    }

    initialize() {
        console.log('SidePanel.initialize() called');
        this.setupSidebarPanel();
        this.setupEventListeners();
        console.log('SidePanel.initialize() completed');
    }

    setupEventListeners() {
        // Listen for customization changes to update legend
        document.addEventListener('customizationChanged', (event) => {
            console.log('SidePanel: Customization changed, updating legend');
            if (this.activeTab === 'legend') {
                this.updateLegend();
            }
        });
    }

    setupSidebarPanel() {
        console.log('=== SIDEPANEL SETUP START ===');
        const sidebarPanel = document.getElementById('sidebar-panel');
        const closeSidebarBtn = document.getElementById('closeSidebarBtn');
        const tabBtns = document.querySelectorAll('.tab-btn');

        console.log('setupSidebarPanel - sidebarPanel found:', !!sidebarPanel);
        console.log('setupSidebarPanel - closeSidebarBtn found:', !!closeSidebarBtn);
        console.log('setupSidebarPanel - tabBtns found:', tabBtns.length);
        
        if (sidebarPanel) {
            console.log('setupSidebarPanel - current sidebar display:', window.getComputedStyle(sidebarPanel).display);
        }

        // Close sidebar button
        if (closeSidebarBtn) {
            closeSidebarBtn.addEventListener('click', () => {
                console.log('Close sidebar button clicked');
                this.toggle(false);
            });
        }

        // Tab switching
        tabBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const tabName = btn.getAttribute('data-tab');
                console.log('Tab button clicked:', tabName);
                this.showTab(tabName);
            });
        });

        // Set initial sidebar state
        if (sidebarPanel) {
            console.log('Setting initial sidebar state, sidebarOpen:', this.sidebarOpen);
            if (this.sidebarOpen) {
                // Don't call toggle here, just set up initial styling to avoid double-setup
                sidebarPanel.style.display = 'flex';
                sidebarPanel.classList.add('open'); // Add the 'open' class for CSS positioning
                const cyContainer = document.getElementById('cy');
                if (cyContainer) {
                    cyContainer.style.marginRight = '350px';
                    console.log('SidePanel: Initial state set to visible, cy margin set to 350px');
                } else {
                    console.error('SidePanel: cy container not found during initial setup');
                }
                console.log('SidePanel: Initial state set to visible with open class');
            } else {
                sidebarPanel.style.display = 'none';
                sidebarPanel.classList.remove('open');
                console.log('SidePanel: Initial state set to hidden');
            }
        } else {
            console.error('SidePanel: sidebar-panel element not found during setup');
        }
        
        // Initialize default tab
        console.log('Initializing default tab:', this.activeTab);
        this.showTab(this.activeTab);
        console.log('=== SIDEPANEL SETUP END ===');
    }

    toggle(show) {
        console.log('SidePanel: toggle() called with show =', show);
        const sidebarPanel = document.getElementById('sidebar-panel');
        const cyContainer = document.getElementById('cy');
        const showSidePanelCheckbox = document.getElementById('showSidePanel');
        
        if (!sidebarPanel) {
            console.error('SidePanel: sidebar-panel element not found');
            return;
        }
        
        if (!cyContainer) {
            console.error('SidePanel: cy container element not found');
            return;
        }

        this.sidebarOpen = show;
        console.log('SidePanel: sidebarOpen set to', this.sidebarOpen);
        
        if (show) {
            sidebarPanel.style.display = 'flex';
            sidebarPanel.classList.add('open'); // Add the 'open' class for CSS positioning
            cyContainer.style.marginRight = '350px';
            console.log('SidePanel: sidebar shown');
            
            // Debug CSS properties
            const computedStyle = window.getComputedStyle(sidebarPanel);
            console.log('CSS Debug - width:', computedStyle.width);
            console.log('CSS Debug - height:', computedStyle.height);
            console.log('CSS Debug - position:', computedStyle.position);
            console.log('CSS Debug - right:', computedStyle.right);
            console.log('CSS Debug - top:', computedStyle.top);
            console.log('CSS Debug - zIndex:', computedStyle.zIndex);
            console.log('CSS Debug - visibility:', computedStyle.visibility);
            console.log('CSS Debug - opacity:', computedStyle.opacity);
            console.log('CSS Debug - transform:', computedStyle.transform);
            
            // Check bounding box
            const rect = sidebarPanel.getBoundingClientRect();
            console.log('Bounding rect:', {
                width: rect.width,
                height: rect.height,
                top: rect.top,
                left: rect.left,
                right: rect.right,
                bottom: rect.bottom
            });
        } else {
            sidebarPanel.style.display = 'none';
            sidebarPanel.classList.remove('open'); // Remove the 'open' class
            cyContainer.style.marginRight = '0';
            console.log('SidePanel: sidebar hidden');
            
            // Uncheck show side panel checkbox when sidebar is closed
            if (showSidePanelCheckbox) {
                showSidePanelCheckbox.checked = false;
            }
        }

        // Trigger resize event for graph renderer
        this.triggerGraphResize();
    }

    // Public method to ensure sidebar is shown initially
    showInitially() {
        console.log('SidePanel: showInitially() called');
        this.toggle(true);
        this.showTab('properties');
    }

    triggerGraphResize() {
        // Dispatch custom event that graph renderer can listen to
        setTimeout(() => {
            const event = new CustomEvent('sidebarToggled', { 
                detail: { sidebarOpen: this.sidebarOpen } 
            });
            document.dispatchEvent(event);
        }, 300);
    }

    showTab(tabName) {
        console.log('Showing tab:', tabName);
        
        // Special handling for customize tab - ensure it exists
        if (tabName === 'customize') {
            if (!this.enhancedViewCustomizer) {
                console.log('Enhanced view customizer not available, cannot show customize tab');
                return;
            }
            
            // Ensure the tab exists
            const customizeTab = document.getElementById('customize-tab');
            if (!customizeTab) {
                console.log('Customize tab does not exist, creating it...');
                this.enhancedViewCustomizer.createCustomizationTab();
            }
        }
        
        // Update active tab button
        const tabBtns = document.querySelectorAll('.tab-btn');
        tabBtns.forEach(btn => {
            if (btn.getAttribute('data-tab') === tabName) {
                btn.classList.add('active');
                console.log('Tab button activated:', btn);
            } else {
                btn.classList.remove('active');
            }
        });

        // Update active tab content
        const tabPanes = document.querySelectorAll('.tab-pane');
        tabPanes.forEach(pane => {
            if (pane.id === tabName + '-tab') {
                pane.classList.add('active');
                pane.style.display = 'block';
                console.log('Tab pane shown:', pane);
            } else {
                pane.classList.remove('active');
                pane.style.display = 'none';
            }
        });

        this.activeTab = tabName;

        // Special handling for customize tab
        if (tabName === 'customize' && this.enhancedViewCustomizer) {
            console.log('Calling enhancedViewCustomizer.show()');
            this.enhancedViewCustomizer.show();
        }

        // Update legend if legend tab is shown
        if (tabName === 'legend') {
            this.updateLegend();
        }
    }

    showCustomizeTab() {
        // Ensure enhanced view customizer is initialized
        if (!this.enhancedViewCustomizer) {
            console.log('Enhanced view customizer not initialized, cannot show customize tab');
            alert('Enhanced view customizer is not available. Please refresh the page.');
            return;
        }
        
        console.log('Showing enhanced view customizer');
        this.enhancedViewCustomizer.show();
        this.toggle(true);
        this.showTab('customize');
    }

    updateLegend() {
        console.log('Updating legend from customization settings...');
        const legendContent = document.getElementById('legend-content');
        if (!legendContent) return;
        
        // Clear existing content
        legendContent.innerHTML = '';
        
        // Check if we have access to graphCustomization through enhancedViewCustomizer
        let graphCustomization = null;
        if (this.enhancedViewCustomizer && this.enhancedViewCustomizer.graphCustomization) {
            graphCustomization = this.enhancedViewCustomizer.graphCustomization;
        }
        
        if (!graphCustomization) {
            console.warn('GraphCustomization not available for legend');
            this.showEmptyLegend(legendContent);
            return;
        }
        
        // Add node types legend from customization settings
        this.addNodeTypesLegend(legendContent, graphCustomization);
        
        // Add edge types legend from customization settings
        this.addEdgeTypesLegend(legendContent, graphCustomization);
    }

    showEmptyLegend(legendContent) {
        const emptyMessage = document.createElement('p');
        emptyMessage.className = 'empty-legend-message';
        emptyMessage.textContent = 'Legend will be populated when Enhanced View is available.';
        emptyMessage.style.textAlign = 'center';
        emptyMessage.style.color = '#666';
        emptyMessage.style.marginTop = '20px';
        legendContent.appendChild(emptyMessage);
    }

    addNodeTypesLegend(legendContent, graphCustomization) {
        const nodeTypes = graphCustomization.getAllNodeTypes();
        
        if (nodeTypes.length === 0) return;
        
        const nodeTypesHeading = document.createElement('h4');
        nodeTypesHeading.textContent = 'Node Types';
        nodeTypesHeading.style.marginBottom = '10px';
        legendContent.appendChild(nodeTypesHeading);
        
        const nodeTypesList = document.createElement('div');
        nodeTypesList.className = 'legend-list';
        
        // Sort node types by importance (descending) for better presentation
        const sortedNodeTypes = nodeTypes.sort((a, b) => {
            const customA = graphCustomization.getNodeCustomization(a);
            const customB = graphCustomization.getNodeCustomization(b);
            return (customB.importance || 0) - (customA.importance || 0);
        });
        
        sortedNodeTypes.forEach(type => {
            const customization = graphCustomization.getNodeCustomization(type);
            const item = document.createElement('div');
            item.className = 'legend-item';
            
            const shapeContainer = document.createElement('div');
            shapeContainer.className = 'legend-node-shape';
            shapeContainer.setAttribute('data-shape', customization.shape);
            shapeContainer.style.backgroundColor = customization.color;
            shapeContainer.style.display = 'inline-block';
            shapeContainer.style.width = '16px';
            shapeContainer.style.height = '16px';
            shapeContainer.style.marginRight = '8px';
            shapeContainer.style.border = '1px solid #ccc';
            shapeContainer.style.verticalAlign = 'middle';
            
            // Apply shape-specific styling
            this.applyNodeShapeStyle(shapeContainer, customization.shape);
            
            const labelContainer = document.createElement('div');
            labelContainer.className = 'legend-label-container';
            labelContainer.style.display = 'inline-block';
            labelContainer.style.verticalAlign = 'middle';
            
            const typeLabel = document.createElement('div');
            typeLabel.className = 'legend-label';
            typeLabel.textContent = type;
            typeLabel.style.fontWeight = 'bold';
            typeLabel.style.marginBottom = '2px';
            
            const detailsLabel = document.createElement('div');
            detailsLabel.className = 'legend-details';
            detailsLabel.style.fontSize = '0.8em';
            detailsLabel.style.color = '#666';
            
            let sizeInfo = customization.sizeMode === 'dynamic' ? 
                `Dynamic (${customization.dynamicKey || 'attribute'})` : 
                `Static (${customization.staticSize}px)`;
            
            detailsLabel.textContent = `Shape: ${customization.shape}, Size: ${sizeInfo}`;
            
            labelContainer.appendChild(typeLabel);
            labelContainer.appendChild(detailsLabel);
            
            item.appendChild(shapeContainer);
            item.appendChild(labelContainer);
            item.style.marginBottom = '8px';
            nodeTypesList.appendChild(item);
        });
        
        legendContent.appendChild(nodeTypesList);
    }

    addEdgeTypesLegend(legendContent, graphCustomization) {
        const edgeTypes = graphCustomization.getAllEdgeTypes();
        
        if (edgeTypes.length === 0) return;
        
        const edgeTypesHeading = document.createElement('h4');
        edgeTypesHeading.textContent = 'Edge Types';
        edgeTypesHeading.style.marginBottom = '10px';
        edgeTypesHeading.style.marginTop = '20px';
        legendContent.appendChild(edgeTypesHeading);
        
        const edgeTypesList = document.createElement('div');
        edgeTypesList.className = 'legend-list';
        
        // Sort edge types by importance (descending) for better presentation
        const sortedEdgeTypes = edgeTypes.sort((a, b) => {
            const customA = graphCustomization.getEdgeCustomization(a);
            const customB = graphCustomization.getEdgeCustomization(b);
            return (customB.importance || 0) - (customA.importance || 0);
        });
        
        sortedEdgeTypes.forEach(type => {
            const customization = graphCustomization.getEdgeCustomization(type);
            const item = document.createElement('div');
            item.className = 'legend-item';
            
            const edgeContainer = document.createElement('div');
            edgeContainer.className = 'legend-edge-shape';
            edgeContainer.style.display = 'inline-block';
            edgeContainer.style.width = '24px';
            edgeContainer.style.height = '16px';
            edgeContainer.style.marginRight = '8px';
            edgeContainer.style.position = 'relative';
            edgeContainer.style.verticalAlign = 'middle';
            
            // Create edge line
            const edgeLine = document.createElement('div');
            edgeLine.style.position = 'absolute';
            edgeLine.style.top = '50%';
            edgeLine.style.left = '2px';
            edgeLine.style.right = '6px';
            edgeLine.style.height = `${customization.width}px`;
            edgeLine.style.backgroundColor = customization.color;
            edgeLine.style.transform = 'translateY(-50%)';
            
            // Apply line style
            if (customization.style === 'dashed') {
                edgeLine.style.borderTop = `${customization.width}px dashed ${customization.color}`;
                edgeLine.style.backgroundColor = 'transparent';
            } else if (customization.style === 'dotted') {
                edgeLine.style.borderTop = `${customization.width}px dotted ${customization.color}`;
                edgeLine.style.backgroundColor = 'transparent';
            }
            
            // Create arrow (simplified representation)
            const arrow = document.createElement('div');
            arrow.style.position = 'absolute';
            arrow.style.right = '0';
            arrow.style.top = '50%';
            arrow.style.width = '0';
            arrow.style.height = '0';
            arrow.style.transform = 'translateY(-50%)';
            
            this.applyArrowStyle(arrow, customization.arrow, customization.color);
            
            edgeContainer.appendChild(edgeLine);
            edgeContainer.appendChild(arrow);
            
            const labelContainer = document.createElement('div');
            labelContainer.className = 'legend-label-container';
            labelContainer.style.display = 'inline-block';
            labelContainer.style.verticalAlign = 'middle';
            
            const typeLabel = document.createElement('div');
            typeLabel.className = 'legend-label';
            typeLabel.textContent = type;
            typeLabel.style.fontWeight = 'bold';
            typeLabel.style.marginBottom = '2px';
            
            const detailsLabel = document.createElement('div');
            detailsLabel.className = 'legend-details';
            detailsLabel.style.fontSize = '0.8em';
            detailsLabel.style.color = '#666';
            detailsLabel.textContent = `Style: ${sanitize(customization.style)}, Arrow: ${sanitize(customization.arrow)}, Width: ${sanitize(customization.width)}px`;
            
            labelContainer.appendChild(typeLabel);
            labelContainer.appendChild(detailsLabel);
            
            item.appendChild(edgeContainer);
            item.appendChild(labelContainer);
            item.style.marginBottom = '8px';
            edgeTypesList.appendChild(item);
        });
        
        legendContent.appendChild(edgeTypesList);
    }

    applyNodeShapeStyle(element, shape) {
        // Reset any previous styles
        element.style.transform = '';
        element.style.borderRadius = '';
        element.style.border = '';
        element.style.width = '16px';
        element.style.height = '16px';
        
        // Get the color that was already set
        const color = element.style.backgroundColor;
        
        // Apply shape-specific styling
        switch (shape) {
            case 'rectangle':
                element.style.borderRadius = '0';
                break;
                
            case 'round-rectangle':
                element.style.borderRadius = '4px';
                break;
                
            case 'triangle':
            case 'round-triangle':
                element.style.width = '0';
                element.style.height = '0';
                element.style.backgroundColor = 'transparent';
                element.style.border = 'none';
                element.style.borderLeft = '8px solid transparent';
                element.style.borderRight = '8px solid transparent';
                element.style.borderBottom = `14px solid ${color}`;
                if (shape === 'round-triangle') {
                    element.style.filter = 'blur(0.5px)';
                }
                break;
                
            case 'diamond':
            case 'round-diamond':
                element.style.width = '12px';
                element.style.height = '12px';
                element.style.transform = 'rotate(45deg)';
                element.style.borderRadius = shape === 'round-diamond' ? '2px' : '0';
                break;
                
            case 'pentagon':
            case 'round-pentagon':
                // Create a pentagon-like shape using clip-path with fallback
                element.style.position = 'relative';
                element.style.borderRadius = shape === 'round-pentagon' ? '2px' : '0';
                element.style.transform = 'rotate(0deg)';
                // Check if clip-path is supported
                if (CSS.supports('clip-path', 'polygon(50% 0%, 100% 38%, 82% 100%, 18% 100%, 0% 38%)')) {
                    element.style.clipPath = 'polygon(50% 0%, 100% 38%, 82% 100%, 18% 100%, 0% 38%)';
                } else {
                    // Fallback: use a slightly rotated rectangle
                    element.style.transform = 'rotate(36deg)';
                    element.style.borderRadius = '2px';
                }
                break;
                
            case 'hexagon':
            case 'round-hexagon':
                element.style.position = 'relative';
                element.style.borderRadius = shape === 'round-hexagon' ? '2px' : '0';
                // Use clip-path for hexagon with fallback
                if (CSS.supports('clip-path', 'polygon(25% 0%, 75% 0%, 100% 50%, 75% 100%, 25% 100%, 0% 50%)')) {
                    element.style.clipPath = 'polygon(25% 0%, 75% 0%, 100% 50%, 75% 100%, 25% 100%, 0% 50%)';
                } else {
                    // Fallback: use a rotated square
                    element.style.transform = 'rotate(30deg)';
                    element.style.borderRadius = '1px';
                }
                break;
                
            case 'octagon':
            case 'round-octagon':
                element.style.position = 'relative';
                element.style.borderRadius = shape === 'round-octagon' ? '2px' : '0';
                // Use clip-path for octagon with fallback
                if (CSS.supports('clip-path', 'polygon(30% 0%, 70% 0%, 100% 30%, 100% 70%, 70% 100%, 30% 100%, 0% 70%, 0% 30%)')) {
                    element.style.clipPath = 'polygon(30% 0%, 70% 0%, 100% 30%, 100% 70%, 70% 100%, 30% 100%, 0% 70%, 0% 30%)';
                } else {
                    // Fallback: use a rotated square
                    element.style.transform = 'rotate(22.5deg)';
                    element.style.borderRadius = '2px';
                }
                break;
                
            case 'star':
                element.style.position = 'relative';
                // Use clip-path for star with fallback
                if (CSS.supports('clip-path', 'polygon(50% 0%, 61% 35%, 98% 35%, 68% 57%, 79% 91%, 50% 70%, 21% 91%, 32% 57%, 2% 35%, 39% 35%)')) {
                    element.style.clipPath = 'polygon(50% 0%, 61% 35%, 98% 35%, 68% 57%, 79% 91%, 50% 70%, 21% 91%, 32% 57%, 2% 35%, 39% 35%)';
                } else {
                    // Fallback: use a rotated diamond
                    element.style.transform = 'rotate(45deg)';
                    element.style.borderRadius = '1px';
                }
                break;
                
            case 'vee':
                element.style.width = '0';
                element.style.height = '0';
                element.style.backgroundColor = 'transparent';
                element.style.border = 'none';
                element.style.borderLeft = `8px solid ${color}`;
                element.style.borderRight = `8px solid ${color}`;
                element.style.borderBottom = '8px solid transparent';
                break;
                
            case 'tag':
                element.style.position = 'relative';
                element.style.borderRadius = '0 4px 4px 0';
                // Add a small notch on the left for tag appearance
                element.style.position = 'relative';
                element.style.marginLeft = '4px';
                break;
                
            case 'round-tag':
                element.style.position = 'relative';
                element.style.borderRadius = '0 8px 8px 0';
                element.style.marginLeft = '4px';
                break;
                
            case 'barrel':
                element.style.borderRadius = '50% 20% 50% 20%';
                break;
                
            case 'rhomboid':
                element.style.transform = 'skew(-15deg)';
                break;
                
            case 'cut-rectangle':
                if (CSS.supports('clip-path', 'polygon(0 0, calc(100% - 4px) 0, 100% 4px, 100% 100%, 4px 100%, 0 calc(100% - 4px))')) {
                    element.style.clipPath = 'polygon(0 0, calc(100% - 4px) 0, 100% 4px, 100% 100%, 4px 100%, 0 calc(100% - 4px))';
                } else {
                    // Fallback: just a rectangle with slight border-radius
                    element.style.borderRadius = '1px';
                }
                break;
                
            case 'bottom-round-rectangle':
                element.style.borderRadius = '0 0 4px 4px';
                break;
                
            case 'concave-hexagon':
                if (CSS.supports('clip-path', 'polygon(20% 0%, 80% 0%, 100% 20%, 100% 80%, 80% 100%, 20% 100%, 0% 80%, 0% 20%)')) {
                    element.style.clipPath = 'polygon(20% 0%, 80% 0%, 100% 20%, 100% 80%, 80% 100%, 20% 100%, 0% 80%, 0% 20%)';
                } else {
                    // Fallback: regular hexagon
                    element.style.transform = 'rotate(30deg)';
                    element.style.borderRadius = '2px';
                }
                break;
                
            case 'heptagon':
            case 'round-heptagon':
                if (CSS.supports('clip-path', 'polygon(50% 0%, 90% 20%, 90% 60%, 75% 100%, 25% 100%, 10% 60%, 10% 20%)')) {
                    element.style.clipPath = 'polygon(50% 0%, 90% 20%, 90% 60%, 75% 100%, 25% 100%, 10% 60%, 10% 20%)';
                } else {
                    // Fallback: rotated rectangle
                    element.style.transform = 'rotate(25deg)';
                }
                element.style.borderRadius = shape === 'round-heptagon' ? '2px' : '0';
                break;
                
            case 'ellipse':
            default:
                element.style.borderRadius = '50%';
                break;
        }
    }

    applyArrowStyle(element, arrowType, color) {
        switch (arrowType) {
            case 'triangle':
                element.style.borderLeft = `4px solid ${color}`;
                element.style.borderTop = '3px solid transparent';
                element.style.borderBottom = '3px solid transparent';
                break;
            case 'diamond':
                element.style.width = '6px';
                element.style.height = '6px';
                element.style.backgroundColor = color;
                element.style.transform = 'translateY(-50%) rotate(45deg)';
                element.style.right = '-3px';
                break;
            case 'circle':
                element.style.width = '6px';
                element.style.height = '6px';
                element.style.backgroundColor = color;
                element.style.borderRadius = '50%';
                element.style.right = '-3px';
                break;
            case 'vee':
                element.style.borderLeft = `3px solid ${color}`;
                element.style.borderTop = '2px solid transparent';
                element.style.borderBottom = '2px solid transparent';
                element.style.borderRight = `3px solid ${color}`;
                element.style.width = '0';
                element.style.height = '0';
                break;
            case 'none':
                // No arrow
                break;
            default:
                // Default triangle
                element.style.borderLeft = `4px solid ${color}`;
                element.style.borderTop = '3px solid transparent';
                element.style.borderBottom = '3px solid transparent';
                break;
        }
    }

    displayNodeProperties(nodeData) {
        console.log('SidePanel: displayNodeProperties called with:', nodeData);
        const propertyTitle = document.getElementById('property-title');
        const propertyContent = document.getElementById('property-content');
        
        if (!propertyTitle || !propertyContent) {
            console.error('SidePanel: property-title or property-content elements not found');
            return;
        }
        
        // Set title
        propertyTitle.textContent = nodeData.name || 'Node Properties';
        
        // Clear existing content
        propertyContent.innerHTML = '';
        
        // Create main info section
        const mainInfoSection = document.createElement('div');
        mainInfoSection.className = 'property-section';
        
        const mainInfoHeader = document.createElement('h4');
        mainInfoHeader.className = 'section-header';
        mainInfoHeader.textContent = 'Basic Information';
        mainInfoSection.appendChild(mainInfoHeader);
        
        const mainInfoTable = document.createElement('div');
        mainInfoTable.className = 'property-table';
        
        // Add main properties
        this.addPropertyRow(mainInfoTable, 'Name', nodeData.name || nodeData.label || 'N/A');
        this.addPropertyRow(mainInfoTable, 'ID', nodeData.id);
        this.addPropertyRow(mainInfoTable, 'Type', nodeData.type);
        
        mainInfoSection.appendChild(mainInfoTable);
        propertyContent.appendChild(mainInfoSection);
        
        // Create attributes section
        const attributesSection = document.createElement('div');
        attributesSection.className = 'property-section';
        
        const attributesHeader = document.createElement('h4');
        attributesHeader.className = 'section-header';
        attributesHeader.textContent = 'Attributes';
        attributesSection.appendChild(attributesHeader);
        
        const attributesTable = document.createElement('div');
        attributesTable.className = 'property-table';
        
        // Add all other properties as attributes
        const excludedKeys = ['id', 'type', 'name', 'label'];
        const attributeKeys = Object.keys(nodeData).filter(key => !excludedKeys.includes(key));
        
        if (attributeKeys.length > 0) {
            attributeKeys.forEach(key => {
                this.addPropertyRow(attributesTable, key, nodeData[key]);
            });
        } else {
            const emptyRow = document.createElement('div');
            emptyRow.className = 'empty-attributes';
            emptyRow.textContent = 'No additional attributes';
            attributesTable.appendChild(emptyRow);
        }
        
        attributesSection.appendChild(attributesTable);
        propertyContent.appendChild(attributesSection);
    }

    displayEdgeProperties(edgeData) {
        console.log('SidePanel: displayEdgeProperties called with:', edgeData);
        const propertyTitle = document.getElementById('property-title');
        const propertyContent = document.getElementById('property-content');
        
        if (!propertyTitle || !propertyContent) {
            console.error('SidePanel: property-title or property-content elements not found');
            return;
        }
        
        // Set title
        propertyTitle.textContent = `Edge: ${sanitize(edgeData.source)} → ${sanitize(edgeData.target)}`;
        
        // Clear existing content
        propertyContent.innerHTML = '';
        
        // Create main info section
        const mainInfoSection = document.createElement('div');
        mainInfoSection.className = 'property-section';
        
        const mainInfoHeader = document.createElement('h4');
        mainInfoHeader.className = 'section-header';
        mainInfoHeader.textContent = 'Basic Information';
        mainInfoSection.appendChild(mainInfoHeader);
        
        const mainInfoTable = document.createElement('div');
        mainInfoTable.className = 'property-table';
        
        // Add main properties
        this.addPropertyRow(mainInfoTable, 'ID', edgeData.id);
        this.addPropertyRow(mainInfoTable, 'Source', edgeData.source);
        this.addPropertyRow(mainInfoTable, 'Target', edgeData.target);
        this.addPropertyRow(mainInfoTable, 'Type', edgeData.type);
        
        mainInfoSection.appendChild(mainInfoTable);
        propertyContent.appendChild(mainInfoSection);
        
        // Create attributes section
        const attributesSection = document.createElement('div');
        attributesSection.className = 'property-section';
        
        const attributesHeader = document.createElement('h4');
        attributesHeader.className = 'section-header';
        attributesHeader.textContent = 'Attributes';
        attributesSection.appendChild(attributesHeader);
        
        const attributesTable = document.createElement('div');
        attributesTable.className = 'property-table';
        
        // Add all other properties as attributes
        const excludedKeys = ['id', 'type', 'source', 'target'];
        const attributeKeys = Object.keys(edgeData).filter(key => !excludedKeys.includes(key));
        
        if (attributeKeys.length > 0) {
            attributeKeys.forEach(key => {
                this.addPropertyRow(attributesTable, key, edgeData[key]);
            });
        } else {
            const emptyRow = document.createElement('div');
            emptyRow.className = 'empty-attributes';
            emptyRow.textContent = 'No additional attributes';
            attributesTable.appendChild(emptyRow);
        }
        
        attributesSection.appendChild(attributesTable);
        propertyContent.appendChild(attributesSection);
    }

    addPropertyRow(container, key, value) {
        const row = document.createElement('div');
        row.className = 'property-row';
        
        const keyElement = document.createElement('div');
        keyElement.className = 'property-name';
        keyElement.textContent = key;
        
        const valueElement = document.createElement('div');
        valueElement.className = 'property-value';
        
        // Handle different value types
        if (value === null || value === undefined) {
            valueElement.textContent = 'N/A';
            valueElement.className += ' empty-value';
        } else if (typeof value === 'boolean') {
            valueElement.textContent = String(value);
            valueElement.className += ' boolean-value';
        } else if (typeof value === 'number') {
            valueElement.textContent = String(value);
            valueElement.className += ' number-value';
        } else if (typeof value === 'object') {
            valueElement.textContent = JSON.stringify(value, null, 2);
            valueElement.className += ' json-value';
        } else {
            valueElement.textContent = String(value);
        }
        
        row.appendChild(keyElement);
        row.appendChild(valueElement);
        container.appendChild(row);
    }

    clearPropertyDisplay() {
        console.log('SidePanel: clearPropertyDisplay called');
        const propertyTitle = document.getElementById('property-title');
        const propertyContent = document.getElementById('property-content');
        
        if (propertyTitle) {
            propertyTitle.textContent = 'No Selection';
        }
        
        if (propertyContent) {
            propertyContent.innerHTML = '<p class="empty-message">Select a node or edge to view its properties</p>';
        }
    }

    isOpen() {
        return this.sidebarOpen;
    }

    getActiveTab() {
        return this.activeTab;
    }
}
