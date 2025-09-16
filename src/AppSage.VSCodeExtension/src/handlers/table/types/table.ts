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

export interface AppSageTable {
    columns: TableColumn[];
    rows: TableRow[];
    metadata?: {
        title?: string;
        description?: string;
        totalRows?: number;
        source?: string;
    };
}
