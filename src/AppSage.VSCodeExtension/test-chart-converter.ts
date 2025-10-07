import { ChartDataConverter } from './src/handlers/chart/components/chartDataConverter';
import { AppSageChartTable } from './src/handlers/chart/types/chart';

// Simple test data
const testTable: AppSageChartTable = {
    TableName: 'TestData',
    Columns: [
        { Name: 'Category', DataType: 'System.String' },
        { Name: 'Value1', DataType: 'System.Int64' },
        { Name: 'Value2', DataType: 'System.Int64' }
    ],
    Rows: [
        { Category: 'A', Value1: 10, Value2: 20 },
        { Category: 'B', Value1: 15, Value2: 25 },
        { Category: 'C', Value1: 12, Value2: 18 }
    ]
};

console.log('Testing Chart Data Converter...');

// Test bar chart conversion
const barChart = ChartDataConverter.convertToECharts([testTable], ['TestData'], 'bar', {});
console.log('Bar Chart Result:', JSON.stringify(barChart, null, 2));

// Test pie chart conversion
const pieChart = ChartDataConverter.convertToECharts([testTable], ['TestData'], 'pie', {});
console.log('Pie Chart Result:', JSON.stringify(pieChart, null, 2));

console.log('Chart Data Converter tests completed successfully!');
