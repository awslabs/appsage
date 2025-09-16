import { AppSageGraph } from '../types/graph';

export class GraphContentProvider {
    public static parseContent(content: string): AppSageGraph {
        try {
            const parsed = JSON.parse(content);
            return this.validateContent(parsed);
        } catch (error) {
            throw new Error(`Invalid graph content: ${error}`);
        }
    }

    private static validateContent(data: any): AppSageGraph {
        if (!data.Nodes || !Array.isArray(data.Nodes)) {
            throw new Error('Graph must have a Nodes array');
        }
        if (!data.Edges || !Array.isArray(data.Edges)) {
            throw new Error('Graph must have an Edges array');
        }
        return data as AppSageGraph;
    }
}
