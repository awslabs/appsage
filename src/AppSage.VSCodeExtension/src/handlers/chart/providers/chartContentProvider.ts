import * as vscode from 'vscode';
import { AppSageLogger } from '../../../shared/logging';

export class ChartContentProvider {
    private logger: AppSageLogger;
    private componentLogger: any;

    constructor(logger: AppSageLogger) {
        this.logger = logger;
        this.componentLogger = logger.forComponent('ChartContentProvider');
    }

    public validateChartData(content: string): boolean {
        try {
            const parsed = JSON.parse(content);
            
            // Check if it's an array (DataTable array format)
            if (!Array.isArray(parsed)) {
                this.componentLogger?.debug('Content is not an array');
                return false;
            }

            // Validate each table has required structure
            for (const table of parsed) {
                if (!table.hasOwnProperty('TableName') || 
                    !table.hasOwnProperty('Columns') || 
                    !table.hasOwnProperty('Rows') ||
                    !Array.isArray(table.Columns) ||
                    !Array.isArray(table.Rows)) {
                    this.componentLogger?.debug('Table missing required properties', { table });
                    return false;
                }
            }

            this.componentLogger?.info('Chart data validation successful');
            return true;
        } catch (error) {
            this.componentLogger?.error('Chart data validation failed', { error });
            return false;
        }
    }

    public parseChartData(content: string) {
        try {
            const tables = JSON.parse(content);
            
            // Ensure table names are set
            const processedTables = tables.map((table: any, index: number) => ({
                ...table,
                TableName: table.TableName || `table${index + 1}`
            }));

            this.componentLogger?.info('Chart data parsed successfully', { 
                tableCount: processedTables.length 
            });

            return {
                tables: processedTables,
                metadata: {
                    totalTables: processedTables.length,
                    tableNames: processedTables.map((t: any) => t.TableName)
                }
            };
        } catch (error) {
            this.componentLogger?.error('Failed to parse chart data', { error });
            throw new Error(`Invalid chart data format: ${error}`);
        }
    }

    public getTableNames(content: string): string[] {
        try {
            const parsed = this.parseChartData(content);
            return parsed.tables.map((table: any) => table.TableName);
        } catch (error) {
            this.componentLogger?.error('Failed to extract table names', { error });
            return [];
        }
    }

    public getTableSummary(content: string) {
        try {
            const parsed = this.parseChartData(content);
            return parsed.tables.map((table: any) => ({
                name: table.TableName,
                columnCount: table.Columns?.length || 0,
                rowCount: table.Rows?.length || 0,
                columns: table.Columns?.map((col: any) => ({
                    name: col.Name,
                    type: col.DataType
                })) || []
            }));
        } catch (error) {
            this.componentLogger?.error('Failed to generate table summary', { error });
            return [];
        }
    }
}
