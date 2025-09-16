using AppSage.Core.ComplexType.Graph;
using System.Data;

public static class AssemblyQuery2
{
    public static IEnumerable<DataTable> Execute(DirectedGraph graph)
    {
        var results = new List<DataTable>();

        // Get all assemblies in the graph
        var assemblies = graph.Nodes
            .Where(n => n.Type == "Assembly")
            .ToList();

        // Create main assemblies table
        var assembliesTable = new DataTable("AllAssemblies");
        assembliesTable.Columns.Add("AssemblyName", typeof(string));
        assembliesTable.Columns.Add("AssemblyVersion", typeof(string));
        assembliesTable.Columns.Add("IsFrameworkAssembly", typeof(string));
        assembliesTable.Columns.Add("ReferencingCount", typeof(int));

        foreach (var assembly in assemblies.OrderBy(a => a.Name))
        {
            string assemblyName = assembly.Attributes.ContainsKey("AssemblyName")
                ? assembly.Attributes["AssemblyName"]
                : assembly.Name;

            string version = assembly.Attributes.ContainsKey("AssebmlyVersion")
                ? assembly.Attributes["AssebmlyVersion"]
                : "Unknown";

            string isFramework = assembly.Attributes.ContainsKey("AssemblyIsFramework")
                ? assembly.Attributes["AssemblyIsFramework"]
                : "Unknown";

            // Count total references to this assembly
            var referencingCount = graph.GetPredecessors(assembly).Count();

            assembliesTable.Rows.Add(assemblyName, version, isFramework, referencingCount);
        }

        results.Add(assembliesTable);

        // Create simple summary
        var summaryTable = new DataTable("Summary");
        summaryTable.Columns.Add("Metric", typeof(string));
        summaryTable.Columns.Add("Value", typeof(string));

        var totalCount = assemblies.Count;
        var frameworkCount = assemblies.Count(a =>
            a.Attributes.ContainsKey("AssemblyIsFramework") &&
            a.Attributes["AssemblyIsFramework"] == "True");

        summaryTable.Rows.Add("Total Assemblies", totalCount.ToString());
        summaryTable.Rows.Add("Framework Assemblies", frameworkCount.ToString());
        summaryTable.Rows.Add("Third-Party Assemblies", (totalCount - frameworkCount).ToString());

        results.Add(summaryTable);

        return results;
    }
}