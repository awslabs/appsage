/**
 * Enhanced View Customization Panel
 * Manages the UI for customizing node and edge appearances
 */

class EnhancedViewCustomizer {
    constructor(vscode, graphCustomization) {
        this.vscode = vscode;
        this.graphCustomization = graphCustomization;
        this.isInitialized = false;
    }

    initialize() {
        if (this.isInitialized) return;
        
        console.log('Initializing EnhancedViewCustomizer...');
        
        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                this.createCustomizationTab();
                this.setupEventHandlers();
                this.loadFromState();
                this.isInitialized = true;
                console.log('EnhancedViewCustomizer initialized successfully');
            });
        } else {
            this.createCustomizationTab();
            this.setupEventHandlers();
            this.loadFromState();
            this.isInitialized = true;
            console.log('EnhancedViewCustomizer initialized successfully');
        }
    }

    createCustomizationTab() {
        console.log('Creating customization tab...');
        
        // Add the customize tab to the sidebar
        const tabsContainer = document.querySelector('.tabs');
        if (!tabsContainer) {
            console.error('Tabs container not found');
            return;
        }

        console.log('Tabs container found:', tabsContainer);

        // Check if tab already exists
        const existingTab = document.querySelector('[data-tab="customize"]');
        if (existingTab) {
            console.log('Customize tab already exists');
            // If tab exists but content doesn't, populate it
            this.populateExistingTab();
            return;
        }

        // Create customize tab button
        const customizeTabBtn = document.createElement('button');
        customizeTabBtn.className = 'tab-btn';
        customizeTabBtn.setAttribute('data-tab', 'customize');
        customizeTabBtn.textContent = 'Customize Enhanced View';
        tabsContainer.appendChild(customizeTabBtn);
        
        console.log('Customize tab button added');

        // Create customize tab content
        const tabContent = document.querySelector('.tab-content');
        if (!tabContent) {
            console.error('Tab content container not found');
            return;
        }

        console.log('Tab content container found:', tabContent);

        const customizeTabPane = document.createElement('div');
        customizeTabPane.id = 'customize-tab';
        customizeTabPane.className = 'tab-pane';
        customizeTabPane.innerHTML = this.createCustomizationHTML();
        tabContent.appendChild(customizeTabPane);
        
        console.log('Customize tab content added');
    }

    populateExistingTab() {
        console.log('Populating existing customize tab...');
        
        // Find the existing customize content div
        const customizeContent = document.getElementById('customize-content');
        if (!customizeContent) {
            console.error('Customize content container not found');
            return;
        }

        // Populate the content
        customizeContent.innerHTML = this.createCustomizationHTML();
        console.log('Existing customize tab populated with content');
        
        // Populate the customizations after content is added
        setTimeout(() => {
            this.populateNodeCustomizations();
            this.populateEdgeCustomizations();
        }, 100);
    }

    createCustomizationHTML() {
        return `
            <div class="panel-section">
                <h3>Node Customization</h3>
                <div class="customization-controls">
                    <div class="controls-header">
                        <div class="control-column type-column">Type</div>
                        <div class="control-column color-column">Color</div>
                        <div class="control-column shape-column">Shape</div>
                        <div class="control-column size-column">Size Mode</div>
                        <div class="control-column size-value-column">Size/Attribute</div>
                        <div class="control-column action-column">Actions</div>
                    </div>
                    <div id="node-customization-list" class="customization-list">
                        <!-- Node customization rows will be populated here -->
                    </div>
                    <div class="add-new-row">
                        <input type="text" id="new-node-type" placeholder="Enter node type">
                        <button id="add-node-type" class="add-btn">Add Node Type</button>
                    </div>
                </div>
            </div>

            <div class="panel-section">
                <h3>Edge Customization</h3>
                <div class="customization-controls">
                    <div class="controls-header">
                        <div class="control-column type-column">Type</div>
                        <div class="control-column color-column">Color</div>
                        <div class="control-column style-column">Style</div>
                        <div class="control-column arrow-column">Arrow</div>
                        <div class="control-column width-column">Width</div>
                        <div class="control-column action-column">Actions</div>
                    </div>
                    <div id="edge-customization-list" class="customization-list">
                        <!-- Edge customization rows will be populated here -->
                    </div>
                    <div class="add-new-row">
                        <input type="text" id="new-edge-type" placeholder="Enter edge type">
                        <button id="add-edge-type" class="add-btn">Add Edge Type</button>
                    </div>
                </div>
            </div>

            <div class="panel-section">
                <div class="action-buttons">
                    <button id="reset-to-defaults" class="action-btn">Reset to Defaults</button>
                    <button id="save-customization" class="action-btn primary">Save Customization</button>
                </div>
            </div>
        `;
    }

    setupEventHandlers() {
        // Add node type button
        document.addEventListener('click', (e) => {
            if (e.target.id === 'add-node-type') {
                this.addNewNodeType();
            }
        });

        // Add edge type button
        document.addEventListener('click', (e) => {
            if (e.target.id === 'add-edge-type') {
                this.addNewEdgeType();
            }
        });

        // Reset to defaults
        document.addEventListener('click', (e) => {
            if (e.target.id === 'reset-to-defaults') {
                this.resetToDefaults();
            }
        });

        // Save customization
        document.addEventListener('click', (e) => {
            if (e.target.id === 'save-customization') {
                this.saveCustomization();
            }
        });

        // Handle removal buttons
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('remove-btn')) {
                this.removeCustomization(e.target);
            }
        });

        // Handle input changes
        document.addEventListener('change', (e) => {
            if (e.target.closest('.customization-list')) {
                this.handleCustomizationChange(e.target);
            }
        });
    }

    populateNodeCustomizations() {
        const container = document.getElementById('node-customization-list');
        if (!container) return;

        container.innerHTML = '';
        const nodeTypes = this.graphCustomization.getAllNodeTypes();

        nodeTypes.forEach(nodeType => {
            const customization = this.graphCustomization.getNodeCustomization(nodeType);
            const row = this.createNodeCustomizationRow(nodeType, customization);
            container.appendChild(row);
        });
    }

    createNodeCustomizationRow(nodeType, customization) {
        const row = document.createElement('div');
        row.className = 'customization-row';
        row.setAttribute('data-type', nodeType);

        row.innerHTML = `
            <div class="control-column type-column">
                <span class="type-name">${sanitizeForHTML(nodeType)}</span>
            </div>
            <div class="control-column color-column">
                <input type="color" value="${sanitizeForHTML(customization.color)}" data-property="color">
            </div>
            <div class="control-column shape-column">
                <select data-property="shape">
                    ${this.createShapeOptions(customization.shape)}
                </select>
            </div>
            <div class="control-column size-column">
                <select data-property="sizeMode">
                    <option value="static" ${customization.sizeMode === 'static' ? 'selected' : ''}>Static</option>
                    <option value="dynamic" ${customization.sizeMode === 'dynamic' ? 'selected' : ''}>Dynamic</option>
                </select>
            </div>
            <div class="control-column size-value-column">
                <input type="number" 
                       min="1" max="100" 
                       value="${sanitizeForHTML(customization.staticSize)}" 
                       data-property="staticSize"
                       ${customization.sizeMode === 'dynamic' ? 'style="display:none"' : ''}>
                <input type="text" 
                       value="${sanitizeForHTML(customization.dynamicKey)}" 
                       placeholder="Attribute key" 
                       data-property="dynamicKey"
                       ${customization.sizeMode === 'static' ? 'style="display:none"' : ''}>
            </div>
            <div class="control-column action-column">
                <button class="remove-btn" title="Remove">×</button>
            </div>
        `;

        return row;
    }

    populateEdgeCustomizations() {
        const container = document.getElementById('edge-customization-list');
        if (!container) return;

        container.innerHTML = '';
        const edgeTypes = this.graphCustomization.getAllEdgeTypes();

        edgeTypes.forEach(edgeType => {
            const customization = this.graphCustomization.getEdgeCustomization(edgeType);
            const row = this.createEdgeCustomizationRow(edgeType, customization);
            container.appendChild(row);
        });
    }

    createEdgeCustomizationRow(edgeType, customization) {
        const row = document.createElement('div');
        row.className = 'customization-row';
        row.setAttribute('data-type', edgeType);

        row.innerHTML = `
            <div class="control-column type-column">
                <span class="type-name">${sanitizeForHTML(edgeType)}</span>
            </div>
            <div class="control-column color-column">
                <input type="color" value="${sanitizeForHTML(customization.color)}" data-property="color">
            </div>
            <div class="control-column style-column">
                <select data-property="style">
                    ${this.createEdgeStyleOptions(customization.style)}
                </select>
            </div>
            <div class="control-column arrow-column">
                <select data-property="arrow">
                    ${this.createArrowOptions(customization.arrow)}
                </select>
            </div>
            <div class="control-column width-column">
                <input type="number" min="1" max="10" value="${sanitizeForHTML(customization.width)}" data-property="width">
            </div>
            <div class="control-column action-column">
                <button class="remove-btn" title="Remove">×</button>
            </div>
        `;

        return row;
    }

    createShapeOptions(selectedShape) {
        const shapes = this.graphCustomization.getAvailableNodeShapes();
        return shapes.map(shape => 
            `<option value="${sanitizeForHTML(shape)}" ${shape === selectedShape ? 'selected' : ''}>${sanitizeForHTML(shape)}</option>`
        ).join('');
    }

    createEdgeStyleOptions(selectedStyle) {
        const styles = this.graphCustomization.getAvailableEdgeStyles();
        return styles.map(style => 
            `<option value="${sanitizeForHTML(style)}" ${style === selectedStyle ? 'selected' : ''}>${sanitizeForHTML(style)}</option>`
        ).join('');
    }

    createArrowOptions(selectedArrow) {
        const arrows = this.graphCustomization.getAvailableArrowShapes();
        return arrows.map(arrow => 
            `<option value="${sanitizeForHTML(arrow)}" ${arrow === selectedArrow ? 'selected' : ''}>${sanitizeForHTML(arrow)}</option>`
        ).join('');
    }

    addNewNodeType() {
        const input = document.getElementById('new-node-type');
        const nodeType = input.value.trim();
        
        if (!nodeType) return;
        
        if (this.graphCustomization.getAllNodeTypes().includes(nodeType)) {
            alert('Node type already exists');
            return;
        }

        const defaultCustomization = this.graphCustomization.getDefaultNodeCustomization();
        defaultCustomization.type = nodeType;
        this.graphCustomization.setNodeCustomization(nodeType, defaultCustomization);
        
        this.populateNodeCustomizations();
        input.value = '';
    }

    addNewEdgeType() {
        const input = document.getElementById('new-edge-type');
        const edgeType = input.value.trim();
        
        if (!edgeType) return;
        
        if (this.graphCustomization.getAllEdgeTypes().includes(edgeType)) {
            alert('Edge type already exists');
            return;
        }

        const defaultCustomization = this.graphCustomization.getDefaultEdgeCustomization();
        defaultCustomization.type = edgeType;
        this.graphCustomization.setEdgeCustomization(edgeType, defaultCustomization);
        
        this.populateEdgeCustomizations();
        input.value = '';
    }

    removeCustomization(button) {
        const row = button.closest('.customization-row');
        const nodeType = row.getAttribute('data-type');
        
        if (row.closest('#node-customization-list')) {
            this.graphCustomization.nodeCustomizations.delete(nodeType);
            this.populateNodeCustomizations();
        } else if (row.closest('#edge-customization-list')) {
            this.graphCustomization.edgeCustomizations.delete(nodeType);
            this.populateEdgeCustomizations();
        }
    }

    handleCustomizationChange(input) {
        const row = input.closest('.customization-row');
        const nodeType = row.getAttribute('data-type');
        const property = input.getAttribute('data-property');
        
        if (row.closest('#node-customization-list')) {
            const customization = this.graphCustomization.getNodeCustomization(nodeType);
            customization[property] = input.type === 'number' ? parseInt(input.value) : input.value;
            
            // Handle size mode change
            if (property === 'sizeMode') {
                const staticInput = row.querySelector('[data-property="staticSize"]');
                const dynamicInput = row.querySelector('[data-property="dynamicKey"]');
                
                if (input.value === 'static') {
                    staticInput.style.display = '';
                    dynamicInput.style.display = 'none';
                } else {
                    staticInput.style.display = 'none';
                    dynamicInput.style.display = '';
                }
            }
            
            this.graphCustomization.setNodeCustomization(nodeType, customization);
        } else if (row.closest('#edge-customization-list')) {
            const customization = this.graphCustomization.getEdgeCustomization(nodeType);
            customization[property] = input.type === 'number' ? parseInt(input.value) : input.value;
            this.graphCustomization.setEdgeCustomization(nodeType, customization);
        }
        
        // Apply changes immediately if in enhanced view
        this.applyChangesImmediately();
    }

    resetToDefaults() {
        this.graphCustomization.initializeDefaults();
        this.populateNodeCustomizations();
        this.populateEdgeCustomizations();
        
        // Apply changes immediately
        this.applyChangesImmediately();
    }

    saveCustomization() {
        const data = this.graphCustomization.toJSON();
        this.vscode.postMessage({
            type: 'saveCustomization',
            data: data
        });
        
        // Apply changes immediately if the enhanced view is active
        this.applyChangesImmediately();
        
        // Show save confirmation
        const saveBtn = document.getElementById('save-customization');
        const originalText = saveBtn.textContent;
        saveBtn.textContent = 'Saved!';
        saveBtn.style.backgroundColor = '#28a745';
        
        setTimeout(() => {
            saveBtn.textContent = originalText;
            saveBtn.style.backgroundColor = '';
        }, 2000);
    }

    applyChangesImmediately() {
        console.log('Applying changes immediately...');
        
        // Check if we have access to the global graph functions
        if (typeof window !== 'undefined' && window.applyEnhancedView) {
            console.log('Calling global applyEnhancedView function');
            window.applyEnhancedView();
        } else {
            // Try to trigger the view mode application through a custom event
            console.log('Triggering customization changed event');
            const event = new CustomEvent('customizationChanged', {
                detail: { 
                    customization: this.graphCustomization.toJSON(),
                    source: 'enhancedViewCustomizer'
                }
            });
            document.dispatchEvent(event);
        }
    }

    loadFromState() {
        this.vscode.postMessage({
            type: 'loadCustomization'
        });
    }

    applyLoadedCustomization(data) {
        if (data) {
            this.graphCustomization.fromJSON(data);
            this.populateNodeCustomizations();
            this.populateEdgeCustomizations();
            
            // Dispatch event to notify other components (including legend)
            const event = new CustomEvent('customizationChanged', {
                detail: { 
                    customization: this.graphCustomization.toJSON(),
                    source: 'loadedCustomization'
                }
            });
            document.dispatchEvent(event);
        }
    }

    show() {
        console.log('EnhancedViewCustomizer.show() called');
        
        if (!this.isInitialized) {
            console.log('Not initialized, initializing now...');
            this.initialize();
        }
        
        // Ensure the customize tab is available
        let customizeTab = document.getElementById('customize-tab');
        if (!customizeTab) {
            console.log('Customize tab not found, creating it...');
            this.createCustomizationTab();
            customizeTab = document.getElementById('customize-tab');
        }
        
        if (customizeTab) {
            console.log('Customize tab found, populating...');
            this.populateNodeCustomizations();
            this.populateEdgeCustomizations();
        } else {
            console.error('Failed to create customize tab');
        }
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = EnhancedViewCustomizer;
}
