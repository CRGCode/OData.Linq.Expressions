using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#pragma warning disable 0660,0661
namespace OData.Linq.Expressions
{
    public partial class ODataExpression
    {
        private readonly ODataExpression functionCaller;
        private readonly ODataExpression left;
        private readonly ODataExpression right;
        private readonly ExpressionType @operator = ExpressionType.Default;
        private readonly Type conversionType;

        private string Reference { get; }
        public object Value { get; }
        private ExpressionFunction Function { get; }

        private bool IsValueConversion => conversionType != null;

        public ODataExpression(Expression expression) : this(FromLinqExpression(expression))
        {
        }

        protected internal ODataExpression(ODataExpression expression)
        {
            functionCaller = expression.functionCaller;
            left = expression.left;
            right = expression.right;
            @operator = expression.@operator;
            conversionType = expression.conversionType;

            Reference = expression.Reference;
            Value = expression.Value;
            Function = expression.Function;
        }

        protected internal ODataExpression(object value)
        {
            Value = value;
        }

        protected internal ODataExpression(string reference)
        {
            Reference = reference;
        }

        protected internal ODataExpression(string reference, object value)
        {
            Reference = reference;
            Value = value;
        }

        protected internal ODataExpression(ExpressionFunction function)
        {
            Function = function;
        }

        protected internal ODataExpression(ODataExpression left, ODataExpression right,
            ExpressionType expressionOperator)
        {
            this.left = left;
            this.right = right;
            @operator = expressionOperator;
        }

        protected internal ODataExpression(ODataExpression caller, string reference)
        {
            functionCaller = caller;
            Reference = reference;
        }

        protected internal ODataExpression(ODataExpression caller, ExpressionFunction function)
        {
            functionCaller = caller;
            Function = function;
        }

        protected internal ODataExpression(ODataExpression expr, Type conversionType)
        {
            this.conversionType = conversionType;
            Value = expr;
        }

        internal static ODataExpression FromReference(string reference)
        {
            return new ODataExpression(reference, (object) null);
        }

        internal static ODataExpression FromValue(object value)
        {
            return new ODataExpression(value);
        }

        internal static ODataExpression FromFunction(ExpressionFunction function)
        {
            return new ODataExpression(function);
        }

        internal static ODataExpression FromFunction(string functionName, ODataExpression targetExpression,
            IEnumerable<object> arguments)
        {
            return new ODataExpression(
                targetExpression,
                new ExpressionFunction(functionName, arguments));
        }

        internal static ODataExpression FromFunction(string functionName, ODataExpression targetExpression,
            IEnumerable<Expression> arguments)
        {
            return new ODataExpression(
                targetExpression,
                new ExpressionFunction(functionName, arguments));
        }

        internal static ODataExpression FromLinqExpression(Expression expression)
        {
            return ParseLinqExpression(expression);
        }

        private bool IsNull =>
            Value == null &&
            Reference == null &&
            Function == null &&
            @operator == ExpressionType.Default;

        public string AsString(ISession session)
        {
            return Format(new ExpressionContext(session));
        }
    }

    public partial class ODataExpression<T> : ODataExpression
    {
        public ODataExpression(Expression<Predicate<T>> predicate)
            : base(predicate)
        {
        }

        internal ODataExpression(ODataExpression expression)
            : base(expression)
        {
        }
    }
}
