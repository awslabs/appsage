using AppSage.Web.Components.Filter;
using AppSage.Web.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using System.Reflection;

namespace AppSage.Web.Extensions
{
    public static class ComponentFilterExtensions
    {
        public static async Task<IHtmlContent> FilterPartialAsync<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            string filterGroupId = "default",
            int dropdownWidth = 450) where TModel : class
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                return HtmlString.Empty;

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
                return HtmlString.Empty;

            // Get the property value
            var compiledExpression = expression.Compile();
            var model = htmlHelper.ViewData.Model;
            var filterModel = compiledExpression(model) as FilterModel;
            if (filterModel == null)
                return HtmlString.Empty;

            // Get the FilterId attribute
            var attribute = property.GetCustomAttributes(typeof(IdNameAttribute), true)
                .FirstOrDefault() as IdNameAttribute;

            var filterId = attribute?.Id;
            var displayName = attribute?.Name;

            var viewData = new ViewDataDictionary(htmlHelper.ViewData)
            {
                { ConstString.VIEWDATA_FILTER_ID, filterId },
                { ConstString.VIEWDATA_FILTER_NAME, displayName },
                { ConstString.VIEWDATA_FILTER_GROUP_ID, filterGroupId },
                { ConstString.VIEWDATA_FILTER_WIDTH, dropdownWidth }
            };

            return await htmlHelper.PartialAsync(ConstString.FILTER_PARTIAL_PATH, filterModel, viewData);
        }

        public static async Task<IHtmlContent> FilterPartialAsync<TModel>(
            this IHtmlHelper htmlHelper,
            TModel model,
            string filterId,
            string filterName,
            string filterGroupId = "default",
            int dropdownWidth =450
        )
        {
            var viewData = new ViewDataDictionary(htmlHelper.ViewData)
            {
                {ConstString.VIEWDATA_FILTER_ID, filterId },
                {ConstString.VIEWDATA_FILTER_NAME, filterName },
                {ConstString.VIEWDATA_FILTER_GROUP_ID, filterGroupId },
                {ConstString.VIEWDATA_FILTER_WIDTH, dropdownWidth }
            };

            return await htmlHelper.PartialAsync(ConstString.FILTER_PARTIAL_PATH, model, viewData);
        }
    }
}
