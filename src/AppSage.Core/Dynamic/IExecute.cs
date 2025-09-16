using AppSage.Core.ComplexType.Graph;

namespace AppSage.Core.Dynamic
{
    public interface IExecute
    {
        T Execute<T>(DirectedGraph graph);
    }
}
