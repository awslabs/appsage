using AppSage.Core.ComplexType.Graph;

namespace AppSage.Query
{
    public interface IDynamicCompiler
    {
        T CompileAndExecute<T>(string code, IDirectedGraph sourceGraph);
        Object? CompileAndExecute(string code, IDirectedGraph sourceGraph);
    }
}
