export interface TableColumn {
    field: string;
    headerName: string;
    width?: number;
    sortable?: boolean;
    filter?: boolean;
    type?: 'text' | 'number' | 'date' | 'boolean';
}

export interface TableRow {
    [key: string]: any;
}

export class GridRenderer {
    private gridApi: any;
    private gridColumnApi: any;

    public initialize(container: HTMLElement, columns: TableColumn[], rows: TableRow[]): void {
        // This will be implemented in the webview JavaScript
        // TypeScript interface for type safety
    }

    public updateData(rows: TableRow[]): void {
        // This will be implemented in the webview JavaScript
    }

    public exportToCsv(): void {
        // This will be implemented in the webview JavaScript
    }

    public toggleFilters(): void {
        // This will be implemented in the webview JavaScript
    }

    public search(searchTerm: string): void {
        // This will be implemented in the webview JavaScript
    }
}
