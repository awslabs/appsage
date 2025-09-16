namespace AppSage.Core.ComplexType;
public class Series<T> 
{
    public string Label { get; set; } = string.Empty;
    public List<T> Data { get; set; } = new List<T>();
    public Series( )
    {
         
    }
    public Series(string label,List<T> data)
    {
        Label = label;
        Data = data;
    }
}