using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OData.Linq.Tests")]
namespace OData.Linq
{
    static class Utils
    {
        public static Exception NotSupportedExpression(Expression expression)
        {
            return new NotSupportedException($"Not supported expression of type {expression.GetType()} ({expression.NodeType}): {expression}");
        }

        public static bool IsSystemType(Type type)
        {
            return
                type.FullName.StartsWith("System.") ||
                type.FullName.StartsWith("Microsoft.");
        }
    }
}
