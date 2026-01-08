namespace AppSage.Core.Documentation
{
    public interface IDocumentationProvider
    {
        IEnumerable<IDocument> GetDocuments();
    }
}
