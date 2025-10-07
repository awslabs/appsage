/**
 * ChartControls - Manages the control panel and user interactions
 */
class ChartControls {
    constructor(vscode, chartRenderer) {
        this.vscode = vscode;
        this.chartRenderer = chartRenderer;
        this.currentData = null;
        this.availableTables = [];
        this.availableColumns = [];
        this.selectedColumns = [];
        
        // Initialize logger for this component
        this.logger = new WebViewLogger(vscode, 'ChartControls');
        
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        // Chart type selection
        const chartTypeSelect = document.getElementById('chartTypeSelect');
        if (chartTypeSelect) {
            chartTypeSelect.addEventListener('change', () => {
                this.updateChart();
            });
        }

        // Table selection
        const tableSelect = document.getElementById('tableSelect');
        if (tableSelect) {
            tableSelect.addEventListener('change', () => {
                this.updateColumnData();
                this.updateXAxisSelector();
                this.updateChart();
                this.updateSidebar();
            });
        }

        // Column types multi-select
        this.updateColumnSelector();

        // X-axis selection
        const xAxisSelect = document.getElementById('xAxisSelect');
        if (xAxisSelect) {
            xAxisSelect.addEventListener('change', () => {
                this.updateChart();
            });
        }

        // Customization controls
        const customizationInputs = [
            'chartTitle', 'showLegend', 'legendPosition', 
            'showGrid', 'enableAnimation', 'colorTheme'
        ];
        
        customizationInputs.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.addEventListener('change', () => {
                    this.updateChart();
                });
                
                if (element.type === 'text') {
                    element.addEventListener('input', this.debounce(() => {
                        this.updateChart();
                    }, 500));
                }
            }
        });

        // Control buttons
        const refreshBtn = document.getElementById('refreshBtn');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                this.refreshChart();
            });
        }

        const exportBtn = document.getElementById('exportBtn');
        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                this.exportChart();
            });
        }

        // Sidebar controls
        const closeSidebarBtn = document.getElementById('closeSidebarBtn');
        if (closeSidebarBtn) {
            closeSidebarBtn.addEventListener('click', () => {
                this.hideSidebar();
            });
        }

        this.logger.info('Chart controls initialized');
    }

    updateData(data) {
        this.currentData = data;
        this.availableTables = data.tables || [];
        
        this.populateTableSelector();
        this.updateChart();
        
        this.logger.info('Chart data updated', { 
            tableCount: this.availableTables.length 
        });
    }

    populateTableSelector() {
        const tableSelect = document.getElementById('tableSelect');
        if (!tableSelect) return;

        // Clear existing options
        tableSelect.innerHTML = '';

        // Add table options
        this.availableTables.forEach((table, index) => {
            const option = document.createElement('option');
            option.value = table.TableName;
            option.textContent = `${table.TableName} (${table.Rows?.length || 0} rows)`;
            // Select the first table by default
            if (index === 0) {
                option.selected = true;
            }
            tableSelect.appendChild(option);
        });

        // Initialize columns and X-axis selectors for the first table
        if (this.availableTables.length > 0) {
            this.updateColumnData();
            this.updateXAxisSelector();
        }

        this.logger.debug('Table selector populated', { 
            tableCount: this.availableTables.length 
        });
    }

    updateColumnSelector() {
        const display = document.getElementById('columnTypesDisplay');
        const dropdown = document.getElementById('columnTypesDropdown');

        if (!display || !dropdown) return;

        // Toggle dropdown on display click
        display.addEventListener('click', (e) => {
            e.stopPropagation();
            dropdown.classList.toggle('show');
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!display.contains(e.target) && !dropdown.contains(e.target)) {
                dropdown.classList.remove('show');
            }
        });
    }

    updateColumnData() {
        const selectedTables = this.getSelectedTables();
        if (selectedTables.length === 0) return;

        const table = this.availableTables.find(t => t.TableName === selectedTables[0]);
        if (!table || !table.Columns) return;

        // Get all column names
        this.availableColumns = table.Columns.map(col => col.Name);

        // Select all columns by default
        this.selectedColumns = [...this.availableColumns];

        this.populateColumnsDropdown();
        this.updateColumnsDisplay();
    }

    populateColumnsDropdown() {
        const dropdown = document.getElementById('columnTypesDropdown');
        if (!dropdown) return;

        dropdown.innerHTML = '';

        this.availableColumns.forEach(columnName => {
            const optionDiv = document.createElement('div');
            optionDiv.className = 'multi-select-option';

            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.id = `col-${columnName.replace(/[^a-zA-Z0-9]/g, '_')}`;
            checkbox.value = columnName;
            checkbox.checked = this.selectedColumns.includes(columnName);
            
            checkbox.addEventListener('change', () => {
                if (checkbox.checked) {
                    if (!this.selectedColumns.includes(columnName)) {
                        this.selectedColumns.push(columnName);
                    }
                } else {
                    this.selectedColumns = this.selectedColumns.filter(name => name !== columnName);
                }
                this.updateColumnsDisplay();
                this.updateChart();
            });

            const label = document.createElement('label');
            label.htmlFor = `col-${columnName.replace(/[^a-zA-Z0-9]/g, '_')}`;
            label.textContent = columnName;

            optionDiv.appendChild(checkbox);
            optionDiv.appendChild(label);
            dropdown.appendChild(optionDiv);
        });
    }

    updateColumnsDisplay() {
        const display = document.getElementById('columnTypesDisplay');
        if (!display) return;

        if (this.selectedColumns.length === 0) {
            display.textContent = 'No columns selected';
        } else if (this.selectedColumns.length === this.availableColumns.length) {
            display.textContent = 'All columns';
        } else if (this.selectedColumns.length === 1) {
            display.textContent = this.selectedColumns[0];
        } else {
            display.textContent = `${this.selectedColumns.length} columns selected`;
        }
    }

    updateXAxisSelector() {
        const selectedTables = this.getSelectedTables();
        const xAxisSelect = document.getElementById('xAxisSelect');
        
        if (!xAxisSelect || selectedTables.length === 0) return;

        const table = this.availableTables.find(t => t.TableName === selectedTables[0]);
        if (!table || !table.Columns) return;

        // Clear existing options
        xAxisSelect.innerHTML = '';

        // Add auto option
        const autoOption = document.createElement('option');
        autoOption.value = 'auto';
        autoOption.textContent = 'Auto (First Category Column)';
        autoOption.selected = true;
        xAxisSelect.appendChild(autoOption);

        // Add all columns as X-axis options
        table.Columns.forEach(column => {
            const option = document.createElement('option');
            option.value = column.Name;
            option.textContent = `${column.Name} (${column.DataType?.split('.').pop() || 'Unknown'})`;
            xAxisSelect.appendChild(option);
        });
    }

    getSelectedTables() {
        const tableSelect = document.getElementById('tableSelect');
        if (!tableSelect || !tableSelect.value) return [];

        return [tableSelect.value]; // Return array with single selected table
    }

    getCurrentConfiguration() {
        const chartTypeSelect = document.getElementById('chartTypeSelect');
        const chartTitle = document.getElementById('chartTitle');
        const showLegend = document.getElementById('showLegend');
        const legendPosition = document.getElementById('legendPosition');
        const showGrid = document.getElementById('showGrid');
        const enableAnimation = document.getElementById('enableAnimation');
        const colorTheme = document.getElementById('colorTheme');
        const xAxisSelect = document.getElementById('xAxisSelect');

        return {
            chartType: chartTypeSelect?.value || 'bar',
            selectedTables: this.getSelectedTables(),
            selectedColumns: [...this.selectedColumns],
            xAxisColumn: xAxisSelect?.value || 'auto',
            customizations: {
                chartTitle: chartTitle?.value || '',
                showLegend: showLegend?.checked !== false,
                legendPosition: legendPosition?.value || 'top',
                showGrid: showGrid?.checked !== false,
                enableAnimation: enableAnimation?.checked !== false,
                colorTheme: colorTheme?.value || 'default'
            }
        };
    }

    updateChart() {
        if (!this.currentData) {
            this.logger.warning('No data available for chart update');
            return;
        }

        const config = this.getCurrentConfiguration();
        
        if (config.selectedTables.length === 0) {
            this.chartRenderer.showError('Please select a table to display');
            return;
        }

        const success = this.chartRenderer.updateChart(this.currentData, config);
        
        if (success) {
            this.vscode.postMessage({
                command: 'updateChart',
                chartType: config.chartType,
                selectedTables: config.selectedTables,
                customizations: config.customizations
            });
            
            this.logger.info('Chart updated', { 
                chartType: config.chartType,
                selectedTables: config.selectedTables.length 
            });
        }
    }

    refreshChart() {
        this.logger.info('Chart refresh requested');
        this.vscode.postMessage({ command: 'getTableSummary' });
        
        // Reset to default configuration
        this.resetToDefaults();
        this.updateChart();
    }

    exportChart() {
        this.logger.info('Chart export requested');
        this.vscode.postMessage({ 
            command: 'exportChart',
            format: 'png',
            config: this.getCurrentConfiguration()
        });
    }

    resetToDefaults() {
        const chartTypeSelect = document.getElementById('chartTypeSelect');
        const chartTitle = document.getElementById('chartTitle');
        const showLegend = document.getElementById('showLegend');
        const legendPosition = document.getElementById('legendPosition');
        const showGrid = document.getElementById('showGrid');
        const enableAnimation = document.getElementById('enableAnimation');
        const colorTheme = document.getElementById('colorTheme');
        const xAxisSelect = document.getElementById('xAxisSelect');

        if (chartTypeSelect) chartTypeSelect.value = 'bar';
        if (chartTitle) chartTitle.value = '';
        if (showLegend) showLegend.checked = true;
        if (legendPosition) legendPosition.value = 'top';
        if (showGrid) showGrid.checked = true;
        if (enableAnimation) enableAnimation.checked = true;
        if (colorTheme) colorTheme.value = 'default';
        if (xAxisSelect) xAxisSelect.value = 'auto';

        // Select first table by default
        const tableSelect = document.getElementById('tableSelect');
        if (tableSelect && tableSelect.options.length > 0) {
            tableSelect.selectedIndex = 0;
        }

        // Reset columns to all selected
        this.selectedColumns = [...this.availableColumns];
        this.populateColumnsDropdown();
        this.updateColumnsDisplay();

        this.logger.debug('Controls reset to defaults');
    }

    showSidebar() {
        const sidebar = document.getElementById('sidebar');
        if (sidebar) {
            sidebar.classList.add('show');
            this.updateSidebar();
        }
    }

    hideSidebar() {
        const sidebar = document.getElementById('sidebar');
        if (sidebar) {
            sidebar.classList.remove('show');
        }
    }

    updateSidebar() {
        this.updateTableInfo();
        this.updateChartStats();
    }

    updateTableInfo() {
        const tableList = document.getElementById('tableList');
        if (!tableList) return;

        const selectedTables = this.getSelectedTables();
        
        if (selectedTables.length === 0) {
            tableList.innerHTML = '<div class="table-item">No table selected</div>';
            return;
        }

        const tableName = selectedTables[0];
        const table = this.availableTables.find(t => t.TableName === tableName);
        
        if (!table) {
            tableList.innerHTML = '<div class="table-item">Selected table not found</div>';
            return;
        }

        const tableInfoHtml = `
            <div class="table-item">
                <div class="table-name">${table.TableName}</div>
                <div class="table-details">
                    ${table.Columns?.length || 0} columns, ${table.Rows?.length || 0} rows
                </div>
                <div class="table-columns">
                    <strong>Columns:</strong>
                    ${table.Columns?.map(col => 
                        `<div class="column-info">${col.Name} (${col.DataType?.split('.').pop() || 'Unknown'})</div>`
                    ).join('') || 'No columns'}
                </div>
            </div>
        `;

        tableList.innerHTML = tableInfoHtml;
    }

    updateChartStats() {
        const statsList = document.getElementById('statsList');
        if (!statsList) return;

        const config = this.getCurrentConfiguration();
        const selectedTables = this.getSelectedTables();
        
        let totalRows = 0;
        let totalColumns = 0;
        
        selectedTables.forEach(tableName => {
            const table = this.availableTables.find(t => t.TableName === tableName);
            if (table) {
                totalRows += table.Rows?.length || 0;
                totalColumns += table.Columns?.length || 0;
            }
        });

        const statsHtml = `
            <div class="stat-item">
                <span class="stat-label">Chart Type</span>
                <span class="stat-value">${config.chartType}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">Selected Table</span>
                <span class="stat-value">${selectedTables.length > 0 ? selectedTables[0] : 'None'}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">Total Rows</span>
                <span class="stat-value">${totalRows}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">Total Columns</span>
                <span class="stat-value">${totalColumns}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">Selected Columns</span>
                <span class="stat-value">${this.selectedColumns.length}/${this.availableColumns.length}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">X-Axis Column</span>
                <span class="stat-value">${config.xAxisColumn === 'auto' ? 'Auto' : config.xAxisColumn}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">Color Theme</span>
                <span class="stat-value">${config.customizations.colorTheme}</span>
            </div>
        `;

        statsList.innerHTML = statsHtml;
    }

    handleError(error) {
        this.logger.error('Chart controls error', { error });
        this.chartRenderer.showError(error);
    }

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Public methods for external control
    setChartType(chartType) {
        const chartTypeSelect = document.getElementById('chartTypeSelect');
        if (chartTypeSelect) {
            chartTypeSelect.value = chartType;
            this.updateChart();
        }
    }

    selectTable(tableName) {
        const tableSelect = document.getElementById('tableSelect');
        if (!tableSelect) return;

        // Find and select the specified table
        for (let i = 0; i < tableSelect.options.length; i++) {
            if (tableSelect.options[i].value === tableName) {
                tableSelect.selectedIndex = i;
                break;
            }
        }
        
        this.updateChart();
        this.updateSidebar();
    }

    setCustomization(key, value) {
        const element = document.getElementById(key);
        if (!element) return;

        if (element.type === 'checkbox') {
            element.checked = value;
        } else {
            element.value = value;
        }

        this.updateChart();
    }
}
