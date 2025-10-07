"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const chartDataConverter_1 = require("./src/handlers/chart/components/chartDataConverter");
// Simple test data
const testTable = {
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
const barChart = chartDataConverter_1.ChartDataConverter.convertToECharts([testTable], ['TestData'], 'bar', {});
console.log('Bar Chart Result:', JSON.stringify(barChart, null, 2));
// Test pie chart conversion
const pieChart = chartDataConverter_1.ChartDataConverter.convertToECharts([testTable], ['TestData'], 'pie', {});
console.log('Pie Chart Result:', JSON.stringify(pieChart, null, 2));
console.log('Chart Data Converter tests completed successfully!');
//# sourceMappingURL=test-chart-converter.js.map