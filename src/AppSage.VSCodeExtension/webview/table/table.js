(function() {
    const vscode = acquireVsCodeApi();
    let gridApi;
    let gridColumnApi;

    function initializeGrid() {
        const gridDiv = document.querySelector('#tableGrid');
        
        const gridOptions = {
            columnDefs: [],
            rowData: [],
            defaultColDef: {
                sortable: true,
                filter: true,
                resizable: true,
                minWidth: 100
            },
            enableRangeSelection: true,
            enableColResize: true,
            animateRows: true,
            pagination: true,
            paginationPageSize: 100
        };

        gridApi = agGrid.createGrid(gridDiv, gridOptions);
        setupControls();
    }

    function setupControls() {
        document.getElementById('exportBtn').addEventListener('click', () => {
            if (gridApi) {
                gridApi.exportDataAsCsv();
            }
        });

        document.getElementById('filterBtn').addEventListener('click', () => {
            if (gridApi) {
                const isFloatingFilter = gridApi.getFloatingFiltersHeight() > 0;
                gridApi.setFloatingFiltersHeight(isFloatingFilter ? 0 : 35);
            }
        });

        document.getElementById('searchInput').addEventListener('input', (e) => {
            if (gridApi) {
                gridApi.setQuickFilter(e.target.value);
            }
        });
    }

    function updateTable(tableData) {
        try {
            const table = JSON.parse(tableData);
            
            if (gridApi) {
                // Update column definitions
                const columnDefs = table.columns.map(col => ({
                    field: col.field,
                    headerName: col.headerName,
                    width: col.width || 150,
                    sortable: col.sortable !== false,
                    filter: col.filter !== false,
                    type: col.type || 'text'
                }));

                gridApi.setColumnDefs(columnDefs);
                gridApi.setRowData(table.rows);
                
                // Auto-size columns to fit content
                gridApi.sizeColumnsToFit();
            }
            
        } catch (error) {
            console.error('Error parsing table data:', error);
        }
    }

    window.addEventListener('message', event => {
        const message = event.data;
        switch (message.type) {
            case 'update':
                updateTable(message.content);
                break;
        }
    });

    document.addEventListener('DOMContentLoaded', () => {
        initializeGrid();
    });
})();
