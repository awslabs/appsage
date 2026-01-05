using AppSage.Core.ComplexType.Graph;

namespace AppSage.MCPServer
{
    public interface IDynamicCompiler
    {
        T CompileAndExecute<T>(string code, IDirectedGraph sourceGraph);
    }
}
