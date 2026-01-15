namespace AppSage.Core.Template
{
    public class TemplateMetadata:ITemplateMetadata
    {
        public TemplateType TemplateType { get; set; }
        public string TemplateId { get; set; }
        public string Description { get; set; }
    }
}
