export interface AppSageChartColumn {
    Name: string;
    DataType: string;
}

export interface AppSageChartTable {
    TableName: string;
    Columns: AppSageChartColumn[];
    Rows: Record<string, any>[];
}

export interface AppSageChart {
    tables: AppSageChartTable[];
}

export interface ChartConfiguration {
    chartType: string;
    selectedTables: string[];
    title?: string;
    customizations: ChartCustomizations;
}

export interface ChartCustomizations {
    colors?: string[];
    theme?: 'light' | 'dark';
    legend?: {
        show: boolean;
        position: 'top' | 'bottom' | 'left' | 'right';
    };
    grid?: {
        show: boolean;
    };
    animation?: boolean;
}

export type ChartType = 
    | 'line'
    | 'bar' 
    | 'pie'
    | 'scatter'
    | 'area'
    | 'heatmap'
    | 'radar'
    | 'gauge';

export interface EChartsOption {
    title?: any;
    tooltip?: any;
    legend?: any;
    grid?: any;
    xAxis?: any;
    yAxis?: any;
    series?: any[];
    color?: string[];
    backgroundColor?: string;
    animation?: boolean;
    visualMap?: any;
    radar?: any;
    [key: string]: any; // Allow additional ECharts properties
}
