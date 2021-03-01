using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OData.Linq.Expressions;

namespace OData.Linq.Fluent
{
    /// <summary>
    /// Provides access to OData operations in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IFluentClient<T, FT>
        where T : class
    {
        /// <summary>
        /// Sets the container for data not covered by the entity properties. Typically used with OData open types.
        /// </summary>
        /// <param name="expression">The filter expression.</param>
        /// <returns>Self.</returns>
        FT WithProperties(Expression<Func<T, IDictionary<string, object>>> expression);

        /// <summary>
        /// Sets the container for media stream content to be retrieved or updated together with standard entity properties.
        /// </summary>
        /// <param name="properties">The media content properties.</param>
        /// <returns>Self.</returns>
        FT WithMedia(IEnumerable<string> properties);
        /// <summary>
        /// Sets the container for media stream content to be retrieved or updated together with standard entity properties.
        /// </summary>
        /// <param name="properties">The media content properties.</param>
        /// <returns>Self.</returns>
        FT WithMedia(params string[] properties);
        /// <summary>
        /// Sets the container for media stream content to be retrieved or updated together with standard entity properties.
        /// </summary>
        /// <param name="properties">The media content properties.</param>
        /// <returns>Self.</returns>
        FT WithMedia(params ODataExpression[] properties);
        /// <summary>
        /// Sets the container for media stream content to be retrieved or updated together with standard entity properties.
        /// </summary>
        /// <param name="properties">The media content properties.</param>
        /// <returns>Self.</returns>
        FT WithMedia(Expression<Func<T, object>> properties);

        /// <summary>
        /// Sets the specified entry key.
        /// </summary>
        /// <param name="entryKey">The entry key.</param>
        /// <returns>Self.</returns>
        FT Key(params object[] entryKey);
        /// <summary>
        /// Sets the specified entry key.
        /// </summary>
        /// <param name="entryKey">The entry key.</param>
        /// <returns>Self.</returns>
        FT Key(IEnumerable<object> entryKey);
        /// <summary>
        /// Sets the specified entry key.
        /// </summary>
        /// <param name="entryKey">The entry key.</param>
        /// <returns>Self.</returns>
        FT Key(IDictionary<string, object> entryKey);
        /// <summary>
        /// Sets the specified entry key.
        /// </summary>
        /// <param name="entryKey">The entry key.</param>
        /// <returns>Self.</returns>
        FT Key(T entryKey);

        /// <summary>
        /// Sets the specified OData filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>Self.</returns>
        FT Filter(string filter);
        /// <summary>
        /// Sets the specified OData filter.
        /// </summary>
        /// <param name="expression">The filter expression.</param>
        /// <returns>Self.</returns>
        FT Filter(ODataExpression expression);
        /// <summary>
        /// Sets the specified OData filter.
        /// </summary>
        /// <param name="expression">The filter expression.</param>
        /// <returns>Self.</returns>
        FT Filter(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Sets the specified OData search.
        /// </summary>
        /// <param name="search">The search term.</param>
        /// <returns>Self.</returns>
        FT Search(string search);

        /// <summary>
        /// Sets the OData function name.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>Self.</returns>
        FT Function(string functionName);
        /// <summary>
        /// Sets the OData action name.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        /// <returns>Self.</returns>
        FT Action(string actionName);

        /// <summary>
        /// Skips the specified number of entries from the result.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>Self.</returns>
        FT Skip(long count);

        /// <summary>
        /// Limits the number of results with the specified value.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns>Self.</returns>
        FT Top(long count);

        /// <summary>
        /// Expands top level of all associations.
        /// </summary>
        /// <param name="expandOptions">The <see cref="ODataExpandOptions"/>.</param>
        /// <returns>Self.</returns>
        FT Expand(ODataExpandOptions expandOptions);
        /// <summary>
        /// Expands the top level of the specified associations.
        /// </summary>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(IEnumerable<string> associations);
        /// <summary>
        /// Expands the number of levels of the specified associations.
        /// </summary>
        /// <param name="expandOptions">The <see cref="ODataExpandOptions"/>.</param>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(ODataExpandOptions expandOptions, IEnumerable<string> associations);
        /// <summary>
        /// Expands the top level of the specified associations.
        /// </summary>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(params string[] associations);
        /// <summary>
        /// Expands the number of levels of the specified associations.
        /// </summary>
        /// <param name="expandOptions">The <see cref="ODataExpandOptions"/>.</param>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(ODataExpandOptions expandOptions, params string[] associations);
        /// <summary>
        /// Expands the top level of the specified associations.
        /// </summary>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(params ODataExpression[] associations);
        /// <summary>
        /// Expands the number of levels of the specified associations.
        /// </summary>
        /// <param name="expandOptions">The <see cref="ODataExpandOptions"/>.</param>
        /// <param name="associations">The associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(ODataExpandOptions expandOptions, params ODataExpression[] associations);
        /// <summary>
        /// Expands the top level of the specified expression.
        /// </summary>
        /// <param name="expression">The expression for associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(Expression<Func<T, object>> expression);
        /// <summary>
        /// Expands the number of levels of the specified associations.
        /// </summary>
        /// <param name="expandOptions">The <see cref="ODataExpandOptions"/>.</param>
        /// <param name="expression">The expression for associations to expand.</param>
        /// <returns>Self.</returns>
        FT Expand(ODataExpandOptions expandOptions, Expression<Func<T, object>> expression);

        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="columns">The selected columns.</param>
        /// <returns>Self.</returns>
        FT Select(IEnumerable<string> columns);
        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="columns">The selected columns.</param>
        /// <returns>Self.</returns>
        FT Select(params string[] columns);
        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="columns">The selected columns.</param>
        /// <returns>Self.</returns>
        FT Select(params ODataExpression[] columns);
        /// <summary>
        /// Selects the specified result columns.
        /// </summary>
        /// <param name="expression">The expression for the selected columns.</param>
        /// <returns>Self.</returns>
        FT Select(Expression<Func<T, object>> expression);

        /// <summary>
        /// Sorts the result by the specified columns in the specified order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        FT OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        FT OrderBy(params string[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        FT OrderBy(params ODataExpression[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        FT OrderBy(Expression<Func<T, object>> expression);
        /// <summary>
        /// Sorts the result by the specified columns in ascending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        FT ThenBy(Expression<Func<T, object>> expression);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        FT OrderByDescending(params string[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="columns">The sort columns.</param>
        /// <returns>Self.</returns>
        FT OrderByDescending(params ODataExpression[] columns);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        FT OrderByDescending(Expression<Func<T, object>> expression);
        /// <summary>
        /// Sorts the result by the specified columns in descending order.
        /// </summary>
        /// <param name="expression">The expression for the sort columns.</param>
        /// <returns>Self.</returns>
        FT ThenByDescending(Expression<Func<T, object>> expression);

        /// <summary>
        /// Sets the custom query options.
        /// </summary>
        /// <param name="queryOptions">The custom query options string.</param>
        /// <returns>Self.</returns>
        FT QueryOptions(string queryOptions);
        /// <summary>
        /// Sets the custom query options.
        /// </summary>
        /// <param name="queryOptions">The key/value collection of custom query options.</param>
        /// <returns>Self.</returns>
        FT QueryOptions(IDictionary<string, object> queryOptions);
        /// <summary>
        /// Sets the custom query options.
        /// </summary>
        /// <param name="expression">The custom query options expression.</param>
        /// <returns>Self.</returns>
        FT QueryOptions(ODataExpression expression);
        /// <summary>
        /// Sets the custom query options.
        /// </summary>
        /// <param name="expression">The custom query options expression.</param>
        /// <returns>Self.</returns>
        FT QueryOptions<U>(Expression<Func<U, bool>> expression);
    }
}
