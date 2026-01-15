namespace AppSage.Core.Template
{
    public interface ITemplateManager
    {
        /// <summary>
        /// Get all available template metadata
        /// </summary>
        /// <returns>List of template metadata</returns>
        public IEnumerable<ITemplateMetadata> GetTemplateMetadata();

        /// <summary>
        /// Get templates by template Id. 
        /// If Id is null or empty, return all templates.
        /// If Id is a group id, return all templates in that group.
        /// If Id is a template id, return the specific template.
        /// </summary>
        /// <param name="Id">template Id to return. This can be group id, individual id or null</param>
        /// <returns>Selected list of templates</returns>
        public IEnumerable<ITemplate> GetTemplates(string Id);

    }
}
