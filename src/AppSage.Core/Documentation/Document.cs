namespace AppSage.Core.Documentation
{
    public class Document : IDocument
    {
        public string Provider { get; set; }
        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string Content { get; set; }
    }
}
