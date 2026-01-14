
namespace AppSage.Query
{
    public class IQueryTemplate
    {
        public string GroupName { get; set; }

        public string Name { get; set; }
        public QueryOutputType OutputType { get; set; }
        public string Content { get; set; }
    }
}
