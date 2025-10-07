/**
 * ChartRenderer - Core ECharts functionality and rendering logic
 */
class ChartRenderer {
    constructor(vscode) {
        this.vscode = vscode;
        this.chart = null;
        this.container = null;
        this.isInitialized = false;
        this.currentData = null;
        this.currentConfig = {
            chartType: 'bar',
            selectedTables: [],
            customizations: {}
        };
        
        // Color themes
        this.colorThemes = {
            default: ['#5470c6', '#91cc75', '#fac858', '#ee6666', '#73c0de', '#3ba272', '#fc8452', '#9a60b4', '#ea7ccc'],
            blue: ['#1f77b4', '#aec7e8', '#2ca02c', '#98df8a', '#d62728', '#ff9896', '#ff7f0e', '#ffbb78'],
            green: ['#2ca02c', '#98df8a', '#8c564b', '#c49c94', '#e377c2', '#f7b6d3', '#7f7f7f', '#c7c7c7'],
            purple: ['#9467bd', '#c5b0d5', '#8c564b', '#c49c94', '#e377c2', '#f7b6d3', '#bcbd22', '#dbdb8d'],
            orange: ['#ff7f0e', '#ffbb78', '#2ca02c', '#98df8a', '#d62728', '#ff9896', '#9467bd', '#c5b0d5']
        };
        
        // Initialize logger for this component
        this.logger = new WebViewLogger(vscode, 'ChartRenderer');
    }

    initialize(containerId = 'chartContainer') {
        try {
            this.container = document.getElementById(containerId);
            if (!this.container) {
                this.logger.error('Chart container not found', { containerId });
                return false;
            }

            // Check if ECharts is available
            if (typeof echarts === 'undefined') {
                this.logger.error('ECharts library not available');
                this.showError('ECharts library not loaded');
                return false;
            }

            this.chart = echarts.init(this.container);
            this.isInitialized = true;
            
            // Add resize listener
            window.addEventListener('resize', () => {
                if (this.chart) {
                    this.chart.resize();
                }
            });

            this.logger.info('Chart renderer initialized successfully');
            return true;
        } catch (error) {
            this.logger.error('Failed to initialize chart renderer', { error });
            this.showError('Failed to initialize chart renderer');
            return false;
        }
    }

    updateChart(data, config) {
        try {
            if (!this.isInitialized) {
                this.logger.warning('Chart renderer not initialized');
                return false;
            }

            this.currentData = data;
            this.currentConfig = { ...this.currentConfig, ...config };
            
            const option = this.convertDataToEChartsOption(data, this.currentConfig);
            
            this.chart.setOption(option, true); // true = replace previous option
            this.hideError();
            this.hideLoading();
            
            this.logger.info('Chart updated successfully', { 
                chartType: this.currentConfig.chartType,
                tableCount: data.tables?.length || 0 
            });
            
            return true;
        } catch (error) {
            this.logger.error('Failed to update chart', { error });
            this.showError(`Failed to update chart: ${error.message}`);
            return false;
        }
    }

    convertDataToEChartsOption(data, config) {
        const { chartType, selectedTables, selectedColumns, xAxisColumn, customizations } = config;
        
        if (!data.tables || data.tables.length === 0) {
            return this.createEmptyChartOption();
        }

        // Filter tables based on selection
        const filteredTables = data.tables.filter(table => 
            selectedTables.length === 0 || selectedTables.includes(table.TableName)
        );

        if (filteredTables.length === 0) {
            return this.createEmptyChartOption();
        }

        // Apply customizations
        const colors = this.colorThemes[customizations.colorTheme] || this.colorThemes.default;
        const commonOptions = {
            color: colors,
            animation: customizations.enableAnimation !== false,
            backgroundColor: 'transparent'
        };

        const chartOptions = { selectedColumns, xAxisColumn };

        switch (chartType) {
            case 'bar':
                return { ...this.createBarChart(filteredTables, customizations, chartOptions), ...commonOptions };
            case 'line':
                return { ...this.createLineChart(filteredTables, customizations, chartOptions), ...commonOptions };
            case 'pie':
                return { ...this.createPieChart(filteredTables, customizations, chartOptions), ...commonOptions };
            case 'scatter':
                return { ...this.createScatterChart(filteredTables, customizations, chartOptions), ...commonOptions };
            case 'area':
                return { ...this.createAreaChart(filteredTables, customizations, chartOptions), ...commonOptions };
            case 'heatmap':
                return { ...this.createHeatmapChart(filteredTables, customizations, chartOptions), ...commonOptions };
            case 'radar':
                return { ...this.createRadarChart(filteredTables, customizations, chartOptions), ...commonOptions };
            case 'gauge':
                return { ...this.createGaugeChart(filteredTables, customizations, chartOptions), ...commonOptions };
            default:
                return { ...this.createBarChart(filteredTables, customizations, chartOptions), ...commonOptions };
        }
    }

    createBarChart(tables, customizations, chartOptions = {}) {
        const firstTable = tables[0];
        const { selectedColumns, xAxisColumn } = chartOptions;
        
        const numericColumns = this.getNumericColumns(firstTable, selectedColumns);
        const categoryColumn = this.getCategoryColumn(firstTable, xAxisColumn, selectedColumns);

        if (numericColumns.length === 0) {
            return this.createEmptyChartOption('No numeric columns selected for visualization');
        }

        const categories = firstTable.Rows.map(row => 
            row[categoryColumn?.Name || Object.keys(row)[0]] || 'Unknown'
        );

        const series = numericColumns.slice(0, 5).map(col => ({
            name: col.Name,
            type: 'bar',
            data: firstTable.Rows.map(row => this.parseNumericValue(row[col.Name]))
        }));

        return {
            title: {
                text: customizations.chartTitle || firstTable.TableName || 'Bar Chart',
                left: 'center'
            },
            tooltip: {
                trigger: 'axis',
                axisPointer: {
                    type: 'shadow'
                }
            },
            legend: {
                data: series.map(s => s.name),
                show: customizations.showLegend !== false,
                orient: ['left', 'right'].includes(customizations.legendPosition) ? 'vertical' : 'horizontal',
                left: customizations.legendPosition === 'left' ? 'left' : 
                      customizations.legendPosition === 'right' ? 'right' : 'center',
                top: customizations.legendPosition === 'top' ? 'top' : 
                     customizations.legendPosition === 'bottom' ? 'bottom' : undefined
            },
            grid: {
                left: '3%',
                right: '4%',
                bottom: '3%',
                containLabel: true,
                show: customizations.showGrid !== false
            },
            xAxis: {
                type: 'category',
                data: categories
            },
            yAxis: {
                type: 'value'
            },
            series
        };
    }

    createLineChart(tables, customizations, chartOptions = {}) {
        const barChart = this.createBarChart(tables, customizations, chartOptions);
        
        // Convert bar series to line series
        if (barChart.series) {
            barChart.series = barChart.series.map(series => ({
                ...series,
                type: 'line',
                smooth: true
            }));
        }

        if (barChart.title) {
            barChart.title.text = customizations.chartTitle || tables[0]?.TableName || 'Line Chart';
        }

        // Update tooltip for line chart
        barChart.tooltip = {
            trigger: 'axis'
        };

        return barChart;
    }

    createAreaChart(tables, customizations, chartOptions = {}) {
        const lineChart = this.createLineChart(tables, customizations, chartOptions);
        
        // Add area style to line series
        if (lineChart.series) {
            lineChart.series = lineChart.series.map(series => ({
                ...series,
                areaStyle: {}
            }));
        }

        if (lineChart.title) {
            lineChart.title.text = customizations.chartTitle || tables[0]?.TableName || 'Area Chart';
        }

        return lineChart;
    }

    createPieChart(tables, customizations, chartOptions = {}) {
        const firstTable = tables[0];
        const { selectedColumns, xAxisColumn } = chartOptions;
        
        const numericColumns = this.getNumericColumns(firstTable, selectedColumns);
        const categoryColumn = this.getCategoryColumn(firstTable, xAxisColumn, selectedColumns);

        if (numericColumns.length === 0) {
            return this.createEmptyChartOption('No numeric columns selected for visualization');
        }

        const valueColumn = numericColumns[0];
        const data = firstTable.Rows.slice(0, 20).map(row => ({
            name: row[categoryColumn?.Name || Object.keys(row)[0]] || 'Unknown',
            value: this.parseNumericValue(row[valueColumn.Name])
        }));

        return {
            title: {
                text: customizations.chartTitle || firstTable.TableName || 'Pie Chart',
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
                show: customizations.showLegend !== false
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
            }]
        };
    }

    createScatterChart(tables, customizations, chartOptions = {}) {
        const firstTable = tables[0];
        const { selectedColumns } = chartOptions;
        
        const numericColumns = this.getNumericColumns(firstTable, selectedColumns);

        if (numericColumns.length < 2) {
            return this.createEmptyChartOption('Need at least 2 numeric columns for scatter plot');
        }

        const xColumn = numericColumns[0];
        const yColumn = numericColumns[1];

        const data = firstTable.Rows.map(row => [
            this.parseNumericValue(row[xColumn.Name]),
            this.parseNumericValue(row[yColumn.Name])
        ]);

        return {
            title: {
                text: customizations.chartTitle || firstTable.TableName || 'Scatter Chart',
                left: 'center'
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
                symbolSize: 8
            }]
        };
    }

    createHeatmapChart(tables, customizations, chartOptions = {}) {
        // Simplified heatmap implementation
        return {
            title: {
                text: customizations.chartTitle || 'Heatmap Chart',
                left: 'center'
            },
            tooltip: {
                position: 'top'
            },
            series: [{
                type: 'heatmap',
                data: [[0, 0, 5], [0, 1, 1], [1, 0, 3], [1, 1, 2]],
                label: {
                    show: true
                }
            }]
        };
    }

    createRadarChart(tables, customizations, chartOptions = {}) {
        // Simplified radar implementation
        return {
            title: {
                text: customizations.chartTitle || 'Radar Chart',
                left: 'center'
            },
            tooltip: {},
            radar: {
                indicator: [
                    { name: 'Metric 1', max: 100 },
                    { name: 'Metric 2', max: 100 },
                    { name: 'Metric 3', max: 100 }
                ]
            },
            series: [{
                type: 'radar',
                data: [{
                    value: [80, 90, 70],
                    name: 'Series 1'
                }]
            }]
        };
    }

    createGaugeChart(tables, customizations, chartOptions = {}) {
        const firstTable = tables[0];
        const { selectedColumns } = chartOptions;
        
        const numericColumns = this.getNumericColumns(firstTable, selectedColumns);

        if (numericColumns.length === 0 || firstTable.Rows.length === 0) {
            return this.createEmptyChartOption('No numeric columns selected or no data available');
        }

        const valueColumn = numericColumns[0];
        const value = this.parseNumericValue(firstTable.Rows[0][valueColumn.Name]);
        const max = Math.max(100, value * 1.2);

        return {
            title: {
                text: customizations.chartTitle || 'Gauge Chart',
                left: 'center'
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
            }]
        };
    }

    createEmptyChartOption(message = 'No Data Available') {
        return {
            title: {
                text: message,
                left: 'center',
                top: 'center',
                textStyle: {
                    fontSize: 18,
                    color: '#999'
                }
            },
            series: []
        };
    }

    getNumericColumns(table, selectedColumns = null) {
        if (!table.Columns) return [];
        
        let columns = table.Columns.filter(col => 
            col.DataType && (
                col.DataType.includes('Int') || 
                col.DataType.includes('Decimal') || 
                col.DataType.includes('Double') || 
                col.DataType.includes('Float') ||
                col.DataType.includes('Long')
            )
        );

        // Filter by selected columns if specified
        if (selectedColumns && selectedColumns.length > 0) {
            columns = columns.filter(col => selectedColumns.includes(col.Name));
        }

        return columns;
    }

    getCategoryColumn(table, xAxisColumn = 'auto', selectedColumns = null) {
        if (!table.Columns) return null;

        // If specific X-axis column is selected, use it
        if (xAxisColumn && xAxisColumn !== 'auto') {
            const specifiedColumn = table.Columns.find(col => col.Name === xAxisColumn);
            if (specifiedColumn) {
                // Check if it matches selected columns
                if (selectedColumns && selectedColumns.length > 0) {
                    if (selectedColumns.includes(specifiedColumn.Name)) {
                        return specifiedColumn;
                    }
                } else {
                    return specifiedColumn;
                }
            }
        }

        // Auto selection: find first non-numeric column that matches selected columns
        let categoryColumns = table.Columns.filter(col => 
            !this.getNumericColumns(table).some(numCol => numCol.Name === col.Name)
        );

        // Filter by selected columns if specified
        if (selectedColumns && selectedColumns.length > 0) {
            categoryColumns = categoryColumns.filter(col => 
                selectedColumns.includes(col.Name)
            );
        }

        return categoryColumns.length > 0 ? categoryColumns[0] : null;
    }

    parseNumericValue(value) {
        if (typeof value === 'number') return value;
        if (typeof value === 'string') {
            const parsed = parseFloat(value);
            return isNaN(parsed) ? 0 : parsed;
        }
        return 0;
    }

    showLoading(message = 'Loading...') {
        const loadingEl = document.getElementById('loadingMessage');
        if (loadingEl) {
            loadingEl.textContent = message;
            loadingEl.style.display = 'block';
        }
    }

    hideLoading() {
        const loadingEl = document.getElementById('loadingMessage');
        if (loadingEl) {
            loadingEl.style.display = 'none';
        }
    }

    showError(message) {
        const errorEl = document.getElementById('errorMessage');
        const errorText = document.getElementById('errorText');
        if (errorEl && errorText) {
            errorText.textContent = message;
            errorEl.style.display = 'block';
        }
        this.hideLoading();
        
        this.logger.error('Chart error displayed', { message });
    }

    hideError() {
        const errorEl = document.getElementById('errorMessage');
        if (errorEl) {
            errorEl.style.display = 'none';
        }
    }

    resize() {
        if (this.chart) {
            this.chart.resize();
        }
    }

    dispose() {
        if (this.chart) {
            this.chart.dispose();
            this.chart = null;
        }
        this.isInitialized = false;
    }

    getCurrentConfig() {
        return { ...this.currentConfig };
    }

    getCurrentData() {
        return this.currentData;
    }
}
