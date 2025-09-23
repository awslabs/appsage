export enum ViewerType {
    Graph = 'graph',
    Table = 'table'
}

export interface ViewerConfig {
    type: ViewerType;
    fileExtension: string;
    viewType: string;
    displayName: string;
}

export const VIEWER_CONFIGS: ViewerConfig[] = [
    {
        type: ViewerType.Graph,
        fileExtension: '*.appsagegraph',
        viewType: 'appsage.graphViewer',
        displayName: 'AppSage Graph Viewer'
    },
    {
        type: ViewerType.Table,
        fileExtension: '*.appsage.table',
        viewType: 'appsage.tableViewer',
        displayName: 'AppSage Table Viewer'
    }
];
