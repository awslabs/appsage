import { AppSageChartTable, EChartsOption, ChartType } from '../types';

export class ChartDataConverter {
    /**
     * Converts AppSage chart data to ECharts format
     */
    public static convertToECharts(
        tables: AppSageChartTable[],
        selectedTables: string[],
        chartType: ChartType,
        customizations: any = {}
    ): EChartsOption {
        const filteredTables = tables.filter(table => 
            selectedTables.includes(table.TableName || this.getDefaultTableName(tables.indexOf(table)))
        );

        switch (chartType) {
            case 'bar':
                return this.createBarChart(filteredTables, customizations);
            case 'line':
                return this.createLineChart(filteredTables, customizations);
            case 'pie':
                return this.createPieChart(filteredTables, customizations);
            case 'scatter':
                return this.createScatterChart(filteredTables, customizations);
            case 'area':
                return this.createAreaChart(filteredTables, customizations);
            case 'heatmap':
                return this.createHeatmapChart(filteredTables, customizations);
            case 'radar':
                return this.createRadarChart(filteredTables, customizations);
            case 'gauge':
                return this.createGaugeChart(filteredTables, customizations);
            default:
                return this.createBarChart(filteredTables, customizations);
        }
    }

    private static getDefaultTableName(index: number): string {
        return `table${index + 1}`;
    }

    private static createBarChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        if (tables.length === 0) return this.createEmptyChart();

        const firstTable = tables[0];
        const numericColumns = this.getNumericColumns(firstTable);
        const categoryColumn = this.getCategoryColumn(firstTable);

        const categories = firstTable.Rows.map(row => 
            row[categoryColumn?.Name || Object.keys(row)[0]] || 'Unknown'
        );

        const series = numericColumns.map(col => ({
            name: col.Name,
            type: 'bar',
            data: firstTable.Rows.map(row => this.parseNumericValue(row[col.Name]))
        }));

        return {
            title: {
                text: customizations.title || firstTable.TableName || 'Chart'
            },
            tooltip: {
                trigger: 'axis'
            },
            legend: {
                data: series.map(s => s.name),
                show: customizations.legend?.show !== false,
                orient: customizations.legend?.position === 'left' || customizations.legend?.position === 'right' ? 'vertical' : 'horizontal',
                left: customizations.legend?.position === 'left' ? 'left' : customizations.legend?.position === 'right' ? 'right' : 'center',
                top: customizations.legend?.position === 'top' ? 'top' : customizations.legend?.position === 'bottom' ? 'bottom' : undefined
            },
            grid: {
                left: '3%',
                right: '4%',
                bottom: '3%',
                containLabel: true,
                show: customizations.grid?.show !== false
            },
            xAxis: {
                type: 'category',
                data: categories
            },
            yAxis: {
                type: 'value'
            },
            series,
            color: customizations.colors,
            animation: customizations.animation !== false
        };
    }

    private static createLineChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        if (tables.length === 0) return this.createEmptyChart();

        const firstTable = tables[0];
        const numericColumns = this.getNumericColumns(firstTable);
        const categoryColumn = this.getCategoryColumn(firstTable);

        const categories = firstTable.Rows.map(row => 
            row[categoryColumn?.Name || Object.keys(row)[0]] || 'Unknown'
        );

        const series = numericColumns.map(col => ({
            name: col.Name,
            type: 'line',
            data: firstTable.Rows.map(row => this.parseNumericValue(row[col.Name])),
            smooth: true
        }));

        return {
            title: {
                text: customizations.title || firstTable.TableName || 'Line Chart'
            },
            tooltip: {
                trigger: 'axis'
            },
            legend: {
                data: series.map(s => s.name),
                show: customizations.legend?.show !== false
            },
            grid: {
                left: '3%',
                right: '4%',
                bottom: '3%',
                containLabel: true
            },
            xAxis: {
                type: 'category',
                data: categories
            },
            yAxis: {
                type: 'value'
            },
            series,
            color: customizations.colors,
            animation: customizations.animation !== false
        };
    }

    private static createPieChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        if (tables.length === 0) return this.createEmptyChart();

        const firstTable = tables[0];
        const numericColumns = this.getNumericColumns(firstTable);
        const categoryColumn = this.getCategoryColumn(firstTable);

        if (numericColumns.length === 0) return this.createEmptyChart();

        const valueColumn = numericColumns[0];
        const data = firstTable.Rows.map(row => ({
            name: row[categoryColumn?.Name || Object.keys(row)[0]] || 'Unknown',
            value: this.parseNumericValue(row[valueColumn.Name])
        }));

        return {
            title: {
                text: customizations.title || firstTable.TableName || 'Pie Chart',
                left: 'center'
            },
            tooltip: {
                trigger: 'item',
                formatter: '{a} <br/>{b}: {c} ({d}%)'
            },
            legend: {
                orient: 'vertical',
                left: 'left',
                data: data.map(d => d.name),
                show: customizations.legend?.show !== false
            },
            series: [{
                name: valueColumn.Name,
                type: 'pie',
                radius: '50%',
                data,
                emphasis: {
                    itemStyle: {
                        shadowBlur: 10,
                        shadowOffsetX: 0,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }],
            color: customizations.colors,
            animation: customizations.animation !== false
        };
    }

    private static createScatterChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        if (tables.length === 0) return this.createEmptyChart();

        const firstTable = tables[0];
        const numericColumns = this.getNumericColumns(firstTable);

        if (numericColumns.length < 2) return this.createEmptyChart();

        const xColumn = numericColumns[0];
        const yColumn = numericColumns[1];

        const data = firstTable.Rows.map(row => [
            this.parseNumericValue(row[xColumn.Name]),
            this.parseNumericValue(row[yColumn.Name])
        ]);

        return {
            title: {
                text: customizations.title || firstTable.TableName || 'Scatter Chart'
            },
            tooltip: {
                trigger: 'item',
                formatter: `${xColumn.Name}: {c[0]}<br/>${yColumn.Name}: {c[1]}`
            },
            grid: {
                left: '3%',
                right: '4%',
                bottom: '3%',
                containLabel: true
            },
            xAxis: {
                type: 'value',
                name: xColumn.Name
            },
            yAxis: {
                type: 'value',
                name: yColumn.Name
            },
            series: [{
                type: 'scatter',
                data,
                symbolSize: 6
            }],
            color: customizations.colors,
            animation: customizations.animation !== false
        };
    }

    private static createAreaChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        const lineChart = this.createLineChart(tables, customizations);
        
        // Convert line series to area series
        if (lineChart.series) {
            lineChart.series = lineChart.series.map((series: any) => ({
                ...series,
                areaStyle: {}
            }));
        }

        if (lineChart.title) {
            lineChart.title.text = customizations.title || 'Area Chart';
        }

        return lineChart;
    }

    private static createHeatmapChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        if (tables.length === 0) return this.createEmptyChart();

        const firstTable = tables[0];
        const numericColumns = this.getNumericColumns(firstTable);

        if (numericColumns.length < 3) return this.createEmptyChart();

        const xColumn = numericColumns[0];
        const yColumn = numericColumns[1];
        const valueColumn = numericColumns[2];

        const data = firstTable.Rows.map((row, index) => [
            index % 10, // X coordinate
            Math.floor(index / 10), // Y coordinate
            this.parseNumericValue(row[valueColumn.Name])
        ]);

        return {
            title: {
                text: customizations.title || firstTable.TableName || 'Heatmap'
            },
            tooltip: {
                position: 'top',
                formatter: function(params: any) {
                    return `Value: ${params.value[2]}`;
                }
            },
            grid: {
                height: '50%',
                top: '10%'
            },
            xAxis: {
                type: 'category',
                data: Array.from({length: 10}, (_, i) => i.toString())
            },
            yAxis: {
                type: 'category',
                data: Array.from({length: Math.ceil(firstTable.Rows.length / 10)}, (_, i) => i.toString())
            },
            visualMap: {
                min: Math.min(...data.map(d => d[2])),
                max: Math.max(...data.map(d => d[2])),
                calculable: true,
                orient: 'horizontal',
                left: 'center',
                bottom: '15%'
            },
            series: [{
                type: 'heatmap',
                data,
                label: {
                    show: true
                },
                emphasis: {
                    itemStyle: {
                        shadowBlur: 10,
                        shadowColor: 'rgba(0, 0, 0, 0.5)'
                    }
                }
            }],
            animation: customizations.animation !== false
        };
    }

    private static createRadarChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        if (tables.length === 0) return this.createEmptyChart();

        const firstTable = tables[0];
        const numericColumns = this.getNumericColumns(firstTable);

        if (numericColumns.length < 3) return this.createEmptyChart();

        const indicators = numericColumns.map(col => ({
            name: col.Name,
            max: Math.max(...firstTable.Rows.map(row => this.parseNumericValue(row[col.Name])))
        }));

        const data = firstTable.Rows.slice(0, 5).map((row, index) => ({
            value: numericColumns.map(col => this.parseNumericValue(row[col.Name])),
            name: `Series ${index + 1}`
        }));

        return {
            title: {
                text: customizations.title || firstTable.TableName || 'Radar Chart'
            },
            tooltip: {},
            legend: {
                data: data.map(d => d.name),
                show: customizations.legend?.show !== false
            },
            radar: {
                indicator: indicators
            },
            series: [{
                type: 'radar',
                data
            }],
            color: customizations.colors,
            animation: customizations.animation !== false
        };
    }

    private static createGaugeChart(tables: AppSageChartTable[], customizations: any): EChartsOption {
        if (tables.length === 0) return this.createEmptyChart();

        const firstTable = tables[0];
        const numericColumns = this.getNumericColumns(firstTable);

        if (numericColumns.length === 0 || firstTable.Rows.length === 0) return this.createEmptyChart();

        const valueColumn = numericColumns[0];
        const value = this.parseNumericValue(firstTable.Rows[0][valueColumn.Name]);
        const max = Math.max(...firstTable.Rows.map(row => this.parseNumericValue(row[valueColumn.Name])));

        return {
            title: {
                text: customizations.title || firstTable.TableName || 'Gauge Chart'
            },
            tooltip: {
                formatter: '{a} <br/>{b} : {c}'
            },
            series: [{
                name: valueColumn.Name,
                type: 'gauge',
                min: 0,
                max: max,
                detail: {
                    formatter: '{value}'
                },
                data: [{
                    value: value,
                    name: valueColumn.Name
                }]
            }],
            animation: customizations.animation !== false
        };
    }

    private static createEmptyChart(): EChartsOption {
        return {
            title: {
                text: 'No Data Available',
                left: 'center',
                top: 'center'
            },
            series: []
        };
    }

    private static getNumericColumns(table: AppSageChartTable) {
        return table.Columns.filter(col => 
            col.DataType.includes('Int') || 
            col.DataType.includes('Decimal') || 
            col.DataType.includes('Double') || 
            col.DataType.includes('Float') ||
            col.DataType.includes('Long')
        );
    }

    private static getCategoryColumn(table: AppSageChartTable) {
        return table.Columns.find(col => 
            col.DataType.includes('String') ||
            !this.getNumericColumns(table).some(numCol => numCol.Name === col.Name)
        );
    }

    private static parseNumericValue(value: any): number {
        if (typeof value === 'number') return value;
        if (typeof value === 'string') {
            const parsed = parseFloat(value);
            return isNaN(parsed) ? 0 : parsed;
        }
        return 0;
    }
}
