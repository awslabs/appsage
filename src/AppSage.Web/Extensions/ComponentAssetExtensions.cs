using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppSage.Web.Extensions
{
    /// <summary>
    /// Extension methods for managing page assets that should only be rendered once
    /// </summary>
    public static class ComponentAssetExtensions
    {
        private const string ASSET_REGISTRY_KEY = "_RegisteredAssets";

        /// <summary>
        /// Renders an asset only once per request, even if multiple components try to include it
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance</param>
        /// <param name="assetKey">A unique key to identify the asset</param>
        /// <param name="assetContent">The asset content to render as a Razor template</param>
        /// <returns>An IHtmlContent containing the asset if not previously rendered, or empty content if already rendered</returns>
        public static IHtmlContent RenderAssetOnce(
            this IHtmlHelper htmlHelper,
            string assetKey,
            Func<object, IHtmlContent> assetContent)
        {
            // Get the HttpContext items collection
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var items = httpContext.Items;

            // Create the registry if it doesn't exist
            if (!items.ContainsKey(ASSET_REGISTRY_KEY))
            {
                items[ASSET_REGISTRY_KEY] = new HashSet<string>();
            }
             
            // Get the registry of assets that have been rendered
            var registeredAssets = items[ASSET_REGISTRY_KEY] as HashSet<string>;

            // If the asset has already been registered, return empty content
            if (registeredAssets!.Contains(assetKey))
            {
                return HtmlString.Empty;
            }

            // Mark the asset as registered
            registeredAssets.Add(assetKey);

            // Return the asset content
            return assetContent(null);
        }

        /// <summary>
        /// Renders an asset only once per request, with the content provided directly as a string
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance</param>
        /// <param name="assetKey">A unique key to identify the asset</param>
        /// <param name="assetContent">The asset content as a string (should include appropriate HTML tags)</param>
        /// <returns>An IHtmlContent containing the asset if not previously rendered, or empty content if already rendered</returns>
        public static IHtmlContent RenderAssetOnce(
            this IHtmlHelper htmlHelper,
            string assetKey,
            string assetContent)
        {
            return htmlHelper.RenderAssetOnce(assetKey, _ => new HtmlString(assetContent));
        }

        /// <summary>
        /// Checks if an asset with the given key has been registered already
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance</param>
        /// <param name="assetKey">The asset key to check</param>
        /// <returns>True if the asset has been registered, false otherwise</returns>
        public static bool IsAssetRegistered(
            this IHtmlHelper htmlHelper, 
            string assetKey)
        {
            var httpContext = htmlHelper.ViewContext.HttpContext;
            var items = httpContext.Items;

            if (!items.ContainsKey(ASSET_REGISTRY_KEY))
            {
                return false;
            }

            var registeredAssets = items[ASSET_REGISTRY_KEY] as HashSet<string>;
            return registeredAssets!.Contains(assetKey);
        }
    }
}