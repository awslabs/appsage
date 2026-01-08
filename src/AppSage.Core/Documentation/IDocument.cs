namespace AppSage.Core.Documentation
{
    /// <summary>
    /// Represents a document used for documentation purposes.
    /// </summary>
    /// <example>
    /// This may desribe a metric return by a metric provider. 
    /// </example>
    public interface IDocument
    {
        /// <summary>
        /// Full qualified name of the provider that generates this document.
        /// </summary>
        string Provider { get; }

        /// <summary>
        /// Name of the document. It's expected to be unique within the scope of the provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Short AI friendly description of the document and what it contains.
        /// </summary>
        string ShortDescription { get; }

        /// <summary>
        /// Detailed documentation
        /// </summary>
        string Content { get; }
    }
}
