export interface CytoscapeNode {
    data: {
        id: string;
        name: string;
        type: string;
    };
}

export interface CytoscapeEdge {
    data: {
        id: string;
        source: string;
        target: string;
        type?: string;
    };
}

export class CytoscapeRenderer {
    private cy: any;

    public initialize(container: HTMLElement): void {
        // This will be implemented in the webview JavaScript
        // TypeScript interface for type safety
    }

    public renderGraph(nodes: CytoscapeNode[], edges: CytoscapeEdge[]): void {
        // This will be implemented in the webview JavaScript
    }

    public setLayout(layoutName: string): void {
        // This will be implemented in the webview JavaScript
    }

    public fit(): void {
        // This will be implemented in the webview JavaScript
    }

    public reset(): void {
        // This will be implemented in the webview JavaScript
    }
}
