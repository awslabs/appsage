import { AppSageTable } from '../types/table';

export class TableContentProvider {
    public static parseContent(content: string): AppSageTable {
        try {
            const parsed = JSON.parse(content);
            return this.validateContent(parsed);
        } catch (error) {
            throw new Error(`Invalid table content: ${error}`);
        }
    }

    private static validateContent(data: any): AppSageTable {
        if (!data.columns || !Array.isArray(data.columns)) {
            throw new Error('Table must have a columns array');
        }
        if (!data.rows || !Array.isArray(data.rows)) {
            throw new Error('Table must have a rows array');
        }
        return data as AppSageTable;
    }
}
