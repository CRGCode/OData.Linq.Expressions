﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace OData.Linq.Expressions
{
    internal abstract class FunctionToOperatorMapping
    {
        public static bool TryGetOperatorMapping(ODataExpression functionCaller, ExpressionFunction function, AdapterVersion adapterVersion,
            out FunctionToOperatorMapping operatorMapping)
        {
            operatorMapping = DefinedMappings.SingleOrDefault(x => x.CanMap(function.FunctionName, function.Arguments.Count, functionCaller, adapterVersion));
            return operatorMapping != null;
        }

        public abstract string Format(ExpressionContext context, ODataExpression functionCaller, List<ODataExpression> functionArguments);

        protected abstract bool CanMap(string functionName, int argumentCount, ODataExpression functionCaller, AdapterVersion adapterVersion = AdapterVersion.Any);

        private static readonly FunctionToOperatorMapping[] DefinedMappings =
        {
            new InOperatorMapping()
        };
    }

    internal class InOperatorMapping : FunctionToOperatorMapping
    {
        public override string Format(ExpressionContext context, ODataExpression functionCaller, List<ODataExpression> functionArguments)
        {
            var list = functionCaller.Value as IEnumerable;
            if (list == null)
            {
                throw new ArgumentException("Function caller should have a value");
            }
            var listAsString = new StringBuilder();
            var delimiter = string.Empty;
            listAsString.Append("(");
            foreach (var item in list)
            {
                listAsString.Append(delimiter);
                listAsString.Append(ConvertValue(context, item, false));
                delimiter = ",";
            }
            listAsString.Append(")");

            return $"{functionArguments[0].Format(context)} in {listAsString}";
        }

        private  string ConvertValue(ExpressionContext context, object value, bool escapeDataString)
        {
            var type = value?.GetType();

            if (value != null && context.Session.TypeCache.IsEnumType(type))
                value = new ODataEnumValue(value.ToString(), string.Join(".", type.Namespace, type.Name));
            if (value is ODataExpression expression)
                return expression.AsString(context.Session);

            var odataVersion = ODataVersion.V4;// (ODataVersion)Enum.Parse(typeof(ODataVersion), context.Session.Adapter.GetODataVersionString(), false);
            string ConvertValue(object x) => ODataUriUtils.ConvertToUriLiteral(x, odataVersion, null);// context.Session.Adapter.Model as IEdmModel);

            if (value is ODataEnumValue && context.Session.Settings.EnumPrefixFree)
                value = ((ODataEnumValue)value).Value;
            else if (value is DateTime)
                value = new DateTimeOffset((DateTime)value);

            return escapeDataString
                ? Uri.EscapeDataString(ConvertValue(value))
                : ConvertValue(value);
        }


        protected override bool CanMap(string functionName, int argumentCount, ODataExpression functionCaller, AdapterVersion adapterVersion = AdapterVersion.Any)
        {
            return functionName == nameof(Enumerable.Contains) &&
                   argumentCount == 1 &&
                   functionCaller.Value != null &&
                   functionCaller.Value.GetType() != typeof(string) &&
                   IsInstanceOfType(typeof(IEnumerable), functionCaller.Value) &&
                   adapterVersion == AdapterVersion.V4;
        }

        private static bool IsInstanceOfType(Type expectedType, object value)
        {
            if (value == null)
            {
                return false;
            }
            var valueType = value.GetType();
            if (expectedType.IsAssignableFrom(valueType))
            {
                return true;
            }
            if (expectedType.IsGenericType && !expectedType.GenericTypeArguments.Any() && valueType.IsGenericType)
            {
                var genericArgumentTypes = valueType.GenericTypeArguments;
                if (!genericArgumentTypes.Any())
                {
                    genericArgumentTypes = new[] {typeof(object)};
                    valueType = valueType.MakeGenericType(genericArgumentTypes);
                }
                var expectedGenericType = expectedType.MakeGenericType(genericArgumentTypes);
                return expectedGenericType.IsAssignableFrom(valueType);
            }
            return false;
        }
    }
}