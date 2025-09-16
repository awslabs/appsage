using System.Linq.Expressions;
using System.Reflection;

namespace AppSage.Web.Components
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class IdNameAttribute : Attribute
    {
        public string Id { get; }
        public string Name { get; }

        public IdNameAttribute(string id, string name)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static string GetHtmlId<TProperty>(Expression<Func<TProperty>> propertyExpression, object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Get the property info from the expression
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression must be a member expression", nameof(propertyExpression));

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Expression must target a property", nameof(propertyExpression));

            // Get the attribute from the property
            var attribute = propertyInfo.GetCustomAttributes(typeof(IdNameAttribute), true)
                .FirstOrDefault() as IdNameAttribute;

            // Use the ID from the attribute, or fallback to the property name
            string filterId = attribute?.Id ?? propertyInfo.Name.ToLowerInvariant();

            // Generate the HTML ID combining the filter ID and item's hash code
            return $"{filterId}_{item.GetHashCode()}";
        }



    }
}
