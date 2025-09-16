/**
 * Graph Customization Configuration
 * Manages node and edge visual customization for the Enhanced View
 */

class GraphCustomization {
    constructor() {
        this.nodeCustomizations = new Map();
        this.edgeCustomizations = new Map();
        this.initializeDefaults();
    }

    initializeDefaults() {
        // Default node customizations based on the documentation
        const defaultNodes = [
            // Repository level
            { type: 'Repository', color: '#2E8B57', shape: 'ellipse', sizeMode: 'static', staticSize: 80, dynamicKey: '', importance: 10 },
            
            // Solution level
            { type: 'Solution', color: '#4682B4', shape: 'rectangle', sizeMode: 'static', staticSize: 70, dynamicKey: '', importance: 9 },
            
            // Project level
            { type: 'Project', color: '#4169E1', shape: 'round-rectangle', sizeMode: 'dynamic', staticSize: 60, dynamicKey: 'ProjectLinesOfCode', importance: 8 },
            
            // Assembly level
            { type: 'Assembly', color: '#FF8C00', shape: 'triangle', sizeMode: 'static', staticSize: 50, dynamicKey: '', importance: 7 },
            
            // Type level - Classes are most important for analysis
            { type: 'Class', color: '#DC143C', shape: 'rectangle', sizeMode: 'dynamic', staticSize: 45, dynamicKey: 'NameTypeMethodCount', importance: 6 },
            { type: 'Interface', color: '#9932CC', shape: 'diamond', sizeMode: 'dynamic', staticSize: 40, dynamicKey: 'NameTypeMethodCount', importance: 5 },
            { type: 'Struct', color: '#FF69B4', shape: 'octagon', sizeMode: 'static', staticSize: 35, dynamicKey: '', importance: 4 },
            { type: 'Enum', color: '#32CD32', shape: 'pentagon', sizeMode: 'static', staticSize: 30, dynamicKey: '', importance: 3 },
            { type: 'Delegate', color: '#8A2BE2', shape: 'hexagon', sizeMode: 'static', staticSize: 25, dynamicKey: '', importance: 2 },
            
            // Generic and array types
            { type: 'Generic', color: '#20B2AA', shape: 'star', sizeMode: 'static', staticSize: 35, dynamicKey: '', importance: 4 },
            { type: 'Array', color: '#F0E68C', shape: 'vee', sizeMode: 'static', staticSize: 30, dynamicKey: '', importance: 3 },
            
            // Method level
            { type: 'Method', color: '#FA8072', shape: 'ellipse', sizeMode: 'static', staticSize: 20, dynamicKey: '', importance: 1 },
            
            // Ambiguous and miscellaneous
            { type: 'Ambiguous', color: '#A9A9A9', shape: 'round-tag', sizeMode: 'static', staticSize: 25, dynamicKey: '', importance: 1 },
            { type: 'Miscellaneous', color: '#D3D3D3', shape: 'tag', sizeMode: 'static', staticSize: 20, dynamicKey: '', importance: 1 }
        ];

        // Default edge customizations based on the documentation
        const defaultEdges = [
            // Structural relationships - most important
            { type: 'Reside', color: '#4682B4', style: 'solid', arrow: 'triangle', width: 3, importance: 10 },
            { type: 'Refer', color: '#FF6347', style: 'solid', arrow: 'triangle', width: 2, importance: 9 },
            
            // Inheritance relationships - very important for code analysis
            { type: 'Inherit', color: '#32CD32', style: 'solid', arrow: 'triangle-backcurve', width: 3, importance: 8 },
            { type: 'Implement', color: '#9932CC', style: 'dashed', arrow: 'triangle', width: 2, importance: 7 },
            
            // Composition and usage - important for understanding dependencies
            { type: 'Composition', color: '#DC143C', style: 'solid', arrow: 'diamond', width: 2, importance: 6 },
            { type: 'Use', color: '#FF8C00', style: 'solid', arrow: 'triangle', width: 1, importance: 5 },
            
            // Method level relationships
            { type: 'Invoke', color: '#8A2BE2', style: 'solid', arrow: 'triangle', width: 1, importance: 4 },
            { type: 'Access', color: '#20B2AA', style: 'dotted', arrow: 'triangle', width: 1, importance: 3 },
            { type: 'Create', color: '#FF1493', style: 'solid', arrow: 'circle', width: 1, importance: 4 },
            { type: 'Declare', color: '#00CED1', style: 'dotted', arrow: 'triangle', width: 1, importance: 2 },
            
            // General relationships
            { type: 'Has', color: '#708090', style: 'solid', arrow: 'triangle', width: 1, importance: 3 }
        ];

        // Initialize node customizations
        defaultNodes.forEach(node => {
            this.nodeCustomizations.set(node.type, node);
        });

        // Initialize edge customizations
        defaultEdges.forEach(edge => {
            this.edgeCustomizations.set(edge.type, edge);
        });
    }

    // Node customization methods
    getNodeCustomization(type) {
        return this.nodeCustomizations.get(type) || this.getDefaultNodeCustomization();
    }

    setNodeCustomization(type, customization) {
        this.nodeCustomizations.set(type, customization);
    }

    getDefaultNodeCustomization() {
        return {
            type: 'Default',
            color: '#808080',
            shape: 'ellipse',
            sizeMode: 'static',
            staticSize: 30,
            dynamicKey: '',
            importance: 1
        };
    }

    getAllNodeTypes() {
        return Array.from(this.nodeCustomizations.keys());
    }

    // Edge customization methods
    getEdgeCustomization(type) {
        return this.edgeCustomizations.get(type) || this.getDefaultEdgeCustomization();
    }

    setEdgeCustomization(type, customization) {
        this.edgeCustomizations.set(type, customization);
    }

    getDefaultEdgeCustomization() {
        return {
            type: 'Default',
            color: '#CCCCCC',
            style: 'solid',
            arrow: 'triangle',
            width: 1,
            importance: 1
        };
    }

    getAllEdgeTypes() {
        return Array.from(this.edgeCustomizations.keys());
    }

    // Serialization for saving to extension state
    toJSON() {
        return {
            nodes: Object.fromEntries(this.nodeCustomizations),
            edges: Object.fromEntries(this.edgeCustomizations)
        };
    }

    // Deserialization from extension state
    fromJSON(data) {
        if (data && data.nodes) {
            this.nodeCustomizations.clear();
            Object.entries(data.nodes).forEach(([type, customization]) => {
                this.nodeCustomizations.set(type, customization);
            });
        }

        if (data && data.edges) {
            this.edgeCustomizations.clear();
            Object.entries(data.edges).forEach(([type, customization]) => {
                this.edgeCustomizations.set(type, customization);
            });
        }
    }

    // Get available shapes for nodes
    getAvailableNodeShapes() {
        return [
            'ellipse', 'triangle', 'round-triangle', 'rectangle', 'round-rectangle',
            'bottom-round-rectangle', 'cut-rectangle', 'barrel', 'rhomboid', 
            'diamond', 'round-diamond', 'pentagon', 'round-pentagon', 'hexagon',
            'round-hexagon', 'concave-hexagon', 'heptagon', 'round-heptagon',
            'octagon', 'round-octagon', 'star', 'tag', 'round-tag', 'vee'
        ];
    }

    // Get available edge styles
    getAvailableEdgeStyles() {
        return ['solid', 'dotted', 'dashed'];
    }

    // Get available arrow shapes
    getAvailableArrowShapes() {
        return [
            'triangle', 'triangle-tee', 'triangle-cross', 'triangle-backcurve',
            'vee', 'tee', 'square', 'circle', 'diamond', 'chevron', 'none'
        ];
    }

    // Calculate dynamic node size based on attribute values
    calculateDynamicSize(nodes, nodeType, attributeKey) {
        const nodesOfType = nodes.filter(node => node.data.type === nodeType);
        const values = [];

        nodesOfType.forEach(node => {
            // Check if attribute exists in node data directly or in attributes
            let value = node.data[attributeKey];
            if (value === undefined && node.data.attributes && node.data.attributes[attributeKey]) {
                value = node.data.attributes[attributeKey];
            }
            
            if (value !== undefined) {
                const numValue = parseInt(value);
                if (!isNaN(numValue)) {
                    values.push(numValue);
                }
            }
        });

        if (values.length === 0) {
            return new Map(); // Return empty map if no valid values
        }

        const min = Math.min(...values);
        const max = Math.max(...values);
        const range = max - min;
        const sizeMap = new Map();

        nodesOfType.forEach(node => {
            let value = node.data[attributeKey];
            if (value === undefined && node.data.attributes && node.data.attributes[attributeKey]) {
                value = node.data.attributes[attributeKey];
            }
            
            if (value !== undefined) {
                const numValue = parseInt(value);
                if (!isNaN(numValue)) {
                    // Normalize to 20-80 range (leaving room for static sizes)
                    const normalizedSize = range > 0 ? 
                        20 + ((numValue - min) / range) * 60 : 50;
                    sizeMap.set(node.data.id, Math.round(normalizedSize));
                }
            } else {
                // Use mean value for nodes without the attribute
                const mean = values.reduce((sum, val) => sum + val, 0) / values.length;
                const normalizedSize = range > 0 ? 
                    20 + ((mean - min) / range) * 60 : 50;
                sizeMap.set(node.data.id, Math.round(normalizedSize));
            }
        });

        return sizeMap;
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GraphCustomization;
}
