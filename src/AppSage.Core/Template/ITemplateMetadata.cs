namespace AppSage.Core.Template
{
    /// <summary>
    /// Represents metadata information for a template.
    /// </summary>
    public interface ITemplateMetadata
    {
        public TemplateType TemplateType { get; }
        public string TemplateId { get; }
        public string Description { get; }
    }
}
