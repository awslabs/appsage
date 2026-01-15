using AppSage.Core.ComplexType.Graph;

namespace AppSage.Query
{
    public interface IDynamicCompiler
    {
        (T ExecutionResult, string ExecuteMethodComment) CompileAndExecute<T>(string code, IDirectedGraph sourceGraph);
        (object? ExecutionResult, string ExecuteMethodComment) CompileAndExecute(string code, IDirectedGraph sourceGraph);
    }
}
