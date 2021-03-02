using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData;

namespace OData.Linq.Expressions
{
    public partial class ODataExpression
    {
        internal string Format(ExpressionContext context)
        {
            if (context.IsQueryOption && _operator != ExpressionType.Default &&
                _operator != ExpressionType.And && _operator != ExpressionType.Equal)
            {
                throw new InvalidOperationException("Invalid custom query option.");
            }

            if (_operator == ExpressionType.Default && !IsValueConversion)
            {
                return Reference != null ?
                    FormatReference(context) : Function != null ?
                    FormatFunction(context) :
                    FormatValue(context);
            }

            if (IsValueConversion)
            {
                var expr = Value as ODataExpression;
                if (expr.Reference == null && expr.Function == null && !expr.IsValueConversion)
                {
                    if (expr.Value != null && context.Session.TypeCache.IsEnumType(expr.Value.GetType()))
                    {
                        expr = new ODataExpression(expr.Value);
                    }
                    else if (context.Session.TypeCache.TryConvert(expr.Value, _conversionType, out object result))
                    {
                        expr = new ODataExpression(result);
                    }
                }
                return FormatExpression(expr, context);
            }
            if (_operator == ExpressionType.Not || _operator == ExpressionType.Negate)
            {
                var left = FormatExpression(_left, context);
                var op = FormatOperator(context);
                if (NeedsGrouping(_left))
                    return $"{op} ({left})";
                return $"{op} {left}";
            }
            else
            {
                var left = FormatExpression(_left, context);
                var right = FormatExpression(_right, context);
                var op = FormatOperator(context);

                if (context.IsQueryOption)
                {
                    return $"{left}{op}{right}";
                }

                if (NeedsGrouping(_left))
                    left = $"({left})";
                if (NeedsGrouping(_right))
                    right = $"({right})";

                return $"{left} {op} {right}";
            }
        }

        private static string FormatExpression(ODataExpression expr, ExpressionContext context)
        {
            return expr is null ? "null" : expr.Format(context);
        }

        private string FormatReference(ExpressionContext context)
        {
            var elementNames = new List<string>(Reference.Split('.', '/'));
            var segmentNames = BuildReferencePath(new List<string>(),elementNames, context);
            return FormatScope(string.Join("/", segmentNames), context);
        }

        private string FormatFunction(ExpressionContext context)
        {
            var adapterVersion = AdapterVersion.Default; 
            if (FunctionToOperatorMapping.TryGetOperatorMapping(_functionCaller, Function, adapterVersion, out var operatorMapping))
            {
                return FormatMappedOperator(context, operatorMapping);
            }
            if (FunctionMapping.TryGetFunctionMapping(Function.FunctionName, Function.Arguments.Count(), adapterVersion, out var functionMapping))
            {
                return FormatMappedFunction(context, functionMapping);
            }

            if (string.Equals(Function.FunctionName, ODataLiteral.Any, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Function.FunctionName, ODataLiteral.All, StringComparison.OrdinalIgnoreCase))
            {
                return FormatAnyAllFunction(context);
            }
            if (string.Equals(Function.FunctionName, ODataLiteral.IsOf, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Function.FunctionName, ODataLiteral.Cast, StringComparison.OrdinalIgnoreCase))
            {
                return FormatIsOfCastFunction(context);
            }
            if (string.Equals(Function.FunctionName, "get_Item", StringComparison.Ordinal) &&
                Function.Arguments.Count == 1)
            {
                return FormatArrayIndexFunction(context);
            }
            if (string.Equals(Function.FunctionName, "HasFlag", StringComparison.Ordinal) &&
                Function.Arguments.Count == 1)
            {
                return FormatEnumHasFlagFunction(context);
            }
            if (string.Equals(Function.FunctionName, "ToString", StringComparison.Ordinal) &&
                Function.Arguments.Count == 0)
            {
                return FormatToStringFunction(context);
            }
            if (Function.Arguments.Count == 1)
            {
                var val = Function.Arguments.First();
                if (val.Value != null)
                {
                    var formattedVal = FromValue(
                        string.Equals(Function.FunctionName, "ToBoolean", StringComparison.Ordinal) ? Convert.ToBoolean(val.Value) :
                        string.Equals(Function.FunctionName, "ToByte", StringComparison.Ordinal) ? Convert.ToByte(val.Value) :
                        string.Equals(Function.FunctionName, "ToChar", StringComparison.Ordinal) ? Convert.ToChar(val.Value) :
                        string.Equals(Function.FunctionName, "ToDateTime", StringComparison.Ordinal) ? Convert.ToDateTime(val.Value) :
                        string.Equals(Function.FunctionName, "ToDecimal", StringComparison.Ordinal) ? Convert.ToDecimal(val.Value) :
                        string.Equals(Function.FunctionName, "ToDouble", StringComparison.Ordinal) ? Convert.ToDouble(val.Value) :
                        string.Equals(Function.FunctionName, "ToInt16", StringComparison.Ordinal) ? Convert.ToInt16(val.Value) :
                        string.Equals(Function.FunctionName, "ToInt32", StringComparison.Ordinal) ? Convert.ToInt32(val.Value) :
                        string.Equals(Function.FunctionName, "ToInt64", StringComparison.Ordinal) ? Convert.ToInt64(val.Value) :
                        string.Equals(Function.FunctionName, "ToSByte", StringComparison.Ordinal) ? Convert.ToSByte(val.Value) :
                        string.Equals(Function.FunctionName, "ToSingle", StringComparison.Ordinal) ? Convert.ToSingle(val.Value) :
                        string.Equals(Function.FunctionName, "ToString", StringComparison.Ordinal) ? Convert.ToString(val.Value) :
                        string.Equals(Function.FunctionName, "ToUInt16", StringComparison.Ordinal) ? Convert.ToUInt16(val.Value) :
                        string.Equals(Function.FunctionName, "ToUInt32", StringComparison.Ordinal) ? Convert.ToUInt32(val.Value) :
                        string.Equals(Function.FunctionName, "ToUInt64", StringComparison.Ordinal) ? (object)Convert.ToUInt64(val.Value)
                        : null);
                    if (formattedVal.Value != null)
                        return FormatExpression(formattedVal, context);
                }
            }

            throw new NotSupportedException($"The function {Function.FunctionName} is not supported or called with wrong number of arguments");
        }
        
        private string FormatMappedOperator(ExpressionContext context, FunctionToOperatorMapping mapping)
        {
            return mapping.Format(context, _functionCaller, Function.Arguments);
        }
        
        private string FormatMappedFunction(ExpressionContext context, FunctionMapping mapping)
        {
            var mappedFunction = mapping.FunctionMapper(
                Function.FunctionName, _functionCaller, Function.Arguments).Function;
            var formattedArguments = string.Join(",",
                (IEnumerable<object>)mappedFunction.Arguments.Select(x => FormatExpression(x, context)));

            return $"{mappedFunction.FunctionName}({formattedArguments})";
        }

        private string FormatAnyAllFunction(ExpressionContext context)
        {
            var navigationPath = FormatCallerReference();
            //var entityCollection = context.Session.Metadata.NavigateToCollection(context.EntityCollection, navigationPath);

            string formattedArguments;
            if(!Function.Arguments.Any() && string.Equals(Function.FunctionName, ODataLiteral.Any, StringComparison.OrdinalIgnoreCase))
            {
                formattedArguments = string.Empty;
            }
            else
            {
                var targetQualifier = $"x{(context.Session.ArgumentCounter >= 0 ? (1 + context.Session.ArgumentCounter++ % 9).ToString() : string.Empty)}";
                var expressionContext = new ExpressionContext(context.Session, targetQualifier, context.DynamicPropertiesContainerName);
                formattedArguments = $"{targetQualifier}:{FormatExpression(Function.Arguments.First(), expressionContext)}";
            }

            //var formattedNavigationPath = context.Session.Adapter.GetCommandFormatter().FormatNavigationPath(context.EntityCollection, navigationPath);
            return FormatScope($"{navigationPath}/{Function.FunctionName.ToLower()}({formattedArguments})", context);
        }

        private string FormatIsOfCastFunction(ExpressionContext context)
        {
            var formattedArguments = string.Empty;
            if (!ReferenceEquals(Function.Arguments.First(), null) && !Function.Arguments.First().IsNull)
            {
                formattedArguments += FormatExpression(Function.Arguments.First(), new ExpressionContext(context.Session));
                formattedArguments += ",";
            }
            formattedArguments += FormatExpression(Function.Arguments.Last(), new ExpressionContext(context.Session));

            return $"{Function.FunctionName.ToLower()}({formattedArguments})";
        }

        private string FormatEnumHasFlagFunction(ExpressionContext context)
        {
            var value = FormatExpression(Function.Arguments.First(), new ExpressionContext(context.Session));
            return $"{FormatCallerReference()} has {value}";
        }

        private string FormatArrayIndexFunction(ExpressionContext context)
        {
            var propertyName =
                FormatExpression(Function.Arguments.First(), new ExpressionContext(context.Session)).Trim('\'');
            return _functionCaller.Reference == context.DynamicPropertiesContainerName
                ? propertyName
                : $"{FormatCallerReference()}.{propertyName}";
        }

        private string FormatToStringFunction(ExpressionContext context)
        {
            return _functionCaller.Reference != null 
                ? FormatCallerReference() 
                : _functionCaller.FormatValue(context);
        }

        private string FormatValue(ExpressionContext context)
        {
            string ConvertValueToUriLiteral(object value, bool escapeDataString)
            {
                var type = value?.GetType();
                var session = context.Session;

                if (value != null && context.Session.TypeCache.IsEnumType(type))
                    value = new ODataEnumValue(value.ToString(), type.FullName);
                if (value is ODataExpression exp)
                    return exp.AsString(session);

                var odataVersion = ODataVersion.V4;
                string ConvertValue(object x) => ODataUriUtils.ConvertToUriLiteral(x, odataVersion, null);

                if (value is ODataEnumValue enumValue && session.Settings.EnumPrefixFree)
                    value = enumValue.Value;
                else if (value is DateTime dateTime)
                    value = new DateTimeOffset(dateTime);

                return escapeDataString
                    ? Uri.EscapeDataString(ConvertValue(value))
                    : ConvertValue(value);
            }

            if (Value is ODataExpression expression)
            {
                return expression.Format(context);
            }

            {
                if (Value is Type type)
                {
                    var typeName = string.Join(".", type.Namespace, type.Name);
                    return ConvertValueToUriLiteral(typeName, false);
                }
                return ConvertValueToUriLiteral(Value, false);
            }
        }


        private string FormatOperator(ExpressionContext context)
        {
            switch (_operator)
            {
                case ExpressionType.And:
                    return context.IsQueryOption ? "&" : "and";
                case ExpressionType.Or:
                    return "or";
                case ExpressionType.Not:
                    return "not";
                case ExpressionType.Equal:
                    return context.IsQueryOption ? "=" : "eq";
                case ExpressionType.NotEqual:
                    return "ne";
                case ExpressionType.GreaterThan:
                    return "gt";
                case ExpressionType.GreaterThanOrEqual:
                    return "ge";
                case ExpressionType.LessThan:
                    return "lt";
                case ExpressionType.LessThanOrEqual:
                    return "le";
                case ExpressionType.Add:
                    return "add";
                case ExpressionType.Subtract:
                    return "sub";
                case ExpressionType.Multiply:
                    return "mul";
                case ExpressionType.Divide:
                    return "div";
                case ExpressionType.Modulo:
                    return "mod";
                case ExpressionType.Negate:
                    return "-";
                default:
                    return null;
            }
        }

        private IEnumerable<string> BuildReferencePath(List<string> segmentNames, List<string> elementNames, ExpressionContext context)
        {
            if (!elementNames.Any())
            {
                return segmentNames;
            }

            var objectName = elementNames.First();
            if (FunctionMapping.ContainsFunction(elementNames.First(), 0))
            {
                var formattedFunction = FormatAsFunction(objectName, context);
                segmentNames.Add(formattedFunction);
                return BuildReferencePath(segmentNames, elementNames.Skip(1).ToList(), context);
            }

            segmentNames.AddRange(elementNames);
            return BuildReferencePath(segmentNames, new List<string>(), context);
        }

        private string FormatAsFunction(string objectName, ExpressionContext context)
        {
            if (FunctionMapping.TryGetFunctionMapping(objectName, 0, AdapterVersion.Default, out var mapping))
            {
                var mappedFunction = mapping.FunctionMapper(objectName, _functionCaller, null).Function;
                return $"{mappedFunction.FunctionName}({FormatCallerReference()})";
            }

            return null;
        }

        private string FormatCallerReference()
        {
            return _functionCaller.Reference.Replace(".", "/");
        }

        private int GetPrecedence(ExpressionType op)
        {
            switch (op)
            {
                case ExpressionType.Not:
                case ExpressionType.Negate:
                    return 1;
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                    return 2;
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                    return 3;
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return 4;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return 5;
                case ExpressionType.And:
                    return 6;
                case ExpressionType.Or:
                    return 7;
                default:
                    return 0;
            }
        }

        private bool NeedsGrouping(ODataExpression expr)
        {
            if (_operator == ExpressionType.Default)
                return false;
            if (ReferenceEquals(expr, null))
                return false;
            if (expr._operator == ExpressionType.Default)
                return false;

            int outerPrecedence = GetPrecedence(_operator);
            int innerPrecedence = GetPrecedence(expr._operator);
            return outerPrecedence < innerPrecedence;
        }

        private string FormatScope(string text, ExpressionContext context)
        {
            return string.IsNullOrEmpty(context.ScopeQualifier)
                ? text
                : $"{context.ScopeQualifier}/{text}";
        }
    }
}
