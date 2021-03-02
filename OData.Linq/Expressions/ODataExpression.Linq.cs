﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OData.Linq.Extensions;

namespace OData.Linq.Expressions
{
    public partial class ODataExpression
    {
        public static ODataExpression ParseLinqExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ParseMemberExpression(expression);

                case ExpressionType.Call:
                    return ParseCallExpression(expression);

                case ExpressionType.Lambda:
                    return ParseLambdaExpression(expression);

                case ExpressionType.Constant:
                    return ParseConstantExpression(expression);

                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.Negate:
                    return ParseUnaryExpression(expression);

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    return ParseBinaryExpression(expression);

                case ExpressionType.TypeIs:
                    return ParseTypeIsExpression(expression);
                case ExpressionType.TypeAs:
                    return ParseTypeAsExpression(expression);
                case ExpressionType.Parameter:
                    return ParseTypedParameterExpression(expression);

                case ExpressionType.New:
                    return ParseNewExpression(expression);
            }

            throw Utils.NotSupportedExpression(expression);
        }

        private static ODataExpression ParseMemberExpression(Expression expression, string memberNames = null)
        {
            var memberExpression = expression as MemberExpression;
            if (memberExpression.Expression == null)
            {
                return new ODataExpression(EvaluateStaticMember(memberExpression));
            }

            // NOTE: Can't support ITypeCache here as we might be dealing with dynamic types/expressions
            var memberName = memberExpression.Member.GetMappedName();

            memberNames = memberNames == null ? memberName : string.Join(".", memberName, memberNames);
            switch (memberExpression.Expression.NodeType)
            {
                case ExpressionType.Parameter:
                    return FromReference(memberNames);
                case ExpressionType.Constant:
                    return ParseConstantExpression(memberExpression.Expression, memberNames);
                case ExpressionType.MemberAccess:
                    if (FunctionMapping.ContainsFunction(memberName, 0))
                    {
                        return FromFunction(memberName, ParseMemberExpression(memberExpression.Expression), new List<object>());
                    }
                    else
                    {
                        return ParseMemberExpression(memberExpression.Expression as MemberExpression, memberNames);
                    }

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }

        private static ODataExpression ParseCallExpression(Expression expression)
        {
            var callExpression = expression as MethodCallExpression;
            if (callExpression.Object == null)
            {
                var target = callExpression.Arguments.FirstOrDefault();
                if (target != null)
                {
                    var callArguments = string.Equals(callExpression.Method.DeclaringType.FullName, "System.Convert", StringComparison.Ordinal)
                        ? callExpression.Arguments
                        : callExpression.Arguments.Skip(1);
                    return FromFunction(callExpression.Method.Name, ParseLinqExpression(target), callArguments);
                }

                throw Utils.NotSupportedExpression(expression);
            }

            var arguments = new List<object>();
            arguments.AddRange(callExpression.Arguments.Select(ParseCallArgumentExpression));
            switch (callExpression.Object.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = callExpression.Object as MemberExpression;
                    return new ODataExpression(
                        ParseMemberExpression(memberExpression), 
                        new ExpressionFunction(callExpression.Method.Name, arguments));

                case ExpressionType.Call:
                    if (string.Equals(callExpression.Method.Name, nameof(ToString), StringComparison.Ordinal))
                        return ParseCallExpression(callExpression.Object);
                    else
                        return new ODataExpression(
                            new ODataExpression(callExpression.Object), 
                            new ExpressionFunction(callExpression.Method.Name, callExpression.Arguments));

                case ExpressionType.Constant:
                    return new ODataExpression(ParseConstantExpression(callExpression.Object), 
                        new ExpressionFunction(callExpression.Method.Name, arguments));
            }

            throw Utils.NotSupportedExpression(callExpression.Object);
        }

        private static ODataExpression ParseArrayExpression(Expression expression)
        {
            var binaryEpression = expression as BinaryExpression;
            var arrayExpression = ParseMemberExpression(binaryEpression.Left);
            var indexExpression = ParseConstantExpression(binaryEpression.Right);

            return FromValue((arrayExpression.Value as Array).GetValue(int.Parse(indexExpression.Value.ToString())));
        }

        private static ODataExpression ParseCallArgumentExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return ParseConstantExpression(expression);
                case ExpressionType.MemberAccess:
                    return ParseMemberExpression(expression);
                case ExpressionType.ArrayIndex:
                    return ParseArrayExpression(expression);
                case ExpressionType.Convert:
                    return ParseUnaryExpression(expression);
                case ExpressionType.Call:
                    return ParseCallExpression(expression);

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }

        private static ODataExpression ParseLambdaExpression(Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;

            return ParseLinqExpression(lambdaExpression.Body);
        }

        private static ODataExpression ParseConstantExpression(Expression expression, string memberNames = null)
        {
            var constExpression = expression as ConstantExpression;

            if (constExpression.Value is Expression)
            {
                return ParseConstantExpression(constExpression.Value as Expression, memberNames);
            }

            if (constExpression.Type.IsValue() || constExpression.Type == typeof(string))
            {
                return new ODataExpression(constExpression.Value);
            }

            return new ODataExpression(EvaluateConstValue(
                constExpression.Type, constExpression.Value,
                memberNames == null ? new List<string>() : memberNames.Split('.').ToList()));
        }

        private static ODataExpression ParseUnaryExpression(Expression expression)
        {
            var unaryExpression = (expression as UnaryExpression).Operand;
            var odataExpression = ParseLinqExpression(unaryExpression);
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return !odataExpression;
                case ExpressionType.Convert:
                    return new ODataExpression(odataExpression, expression.Type);
                case ExpressionType.Negate:
                    return -odataExpression;
            }

            throw Utils.NotSupportedExpression(expression);
        }

        private static ODataExpression ParseBinaryExpression(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            var leftExpression = ParseLinqExpression(binaryExpression.Left);
            var rightExpression = ParseLinqExpression(binaryExpression.Right);

            if (IsConvertFromCustomEnum(binaryExpression.Left, out var enumType))
            {
                return ParseBinaryExpression(leftExpression, ParseLinqExpression(Expression.Convert(binaryExpression.Right, enumType)), expression);
            }

            if (IsConvertFromCustomEnum(binaryExpression.Right, out enumType))
            {
                return ParseBinaryExpression(ParseLinqExpression(Expression.Convert(binaryExpression.Left, enumType)), rightExpression, expression);
            }

            return ParseBinaryExpression(leftExpression, rightExpression, expression);
        }

        private static ODataExpression ParseBinaryExpression(ODataExpression leftExpression, ODataExpression rightExpression, Expression operandExpression)
        {
            switch (operandExpression.NodeType)
            {
                case ExpressionType.Equal:
                    return leftExpression == rightExpression;
                case ExpressionType.NotEqual:
                    return leftExpression != rightExpression;
                case ExpressionType.LessThan:
                    return leftExpression < rightExpression;
                case ExpressionType.LessThanOrEqual:
                    return leftExpression <= rightExpression;
                case ExpressionType.GreaterThan:
                    return leftExpression > rightExpression;
                case ExpressionType.GreaterThanOrEqual:
                    return leftExpression >= rightExpression;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return leftExpression && rightExpression;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return leftExpression || rightExpression;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return leftExpression + rightExpression;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return leftExpression - rightExpression;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return leftExpression * rightExpression;
                case ExpressionType.Divide:
                    return leftExpression / rightExpression;
                case ExpressionType.Modulo:
                    return leftExpression % rightExpression;
            }

            throw Utils.NotSupportedExpression(operandExpression);
        }

        private static bool IsConvertFromCustomEnum(Expression expression, out Type enumType)
        {
            enumType = null;
            if (expression.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = (expression as UnaryExpression).Operand;
                enumType = unaryExpression.Type;
                return enumType.IsEnumType() && !Utils.IsSystemType(enumType);
            }
            return false;
        }

        private static ODataExpression ParseTypeIsExpression(Expression expression)
        {
            var typeIsExpression = expression as TypeBinaryExpression;
            var targetExpression = ParseLinqExpression(typeIsExpression.Expression);
            return FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = "isof",
                    Arguments = new List<ODataExpression>() { targetExpression, FromValue(typeIsExpression.TypeOperand) },
                });
        }

        private static ODataExpression ParseTypeAsExpression(Expression expression)
        {
            var typeAsExpression = expression as UnaryExpression;
            var targetExpression = ParseLinqExpression(typeAsExpression.Operand);
            return FromFunction(
                new ExpressionFunction()
                {
                    FunctionName = "cast",
                    Arguments = new List<ODataExpression>() { targetExpression, FromValue(typeAsExpression.Type) },
                });
        }

        private static ODataExpression ParseTypedParameterExpression(Expression expression)
        {
            return null;
        }

        private static ODataExpression ParseNewExpression(Expression expression)
        {
            var newExpression = expression as NewExpression;
            return FromValue(newExpression.Constructor.Invoke(newExpression.Arguments.Select(x => ParseLinqExpression(x).Value).ToArray()));
        }

        private static object EvaluateStaticMember(MemberExpression expression)
        {
            object value = null;
            if (expression.Member is FieldInfo)
            {
                var fi = (FieldInfo)expression.Member;
                value = fi.GetValue(null);
            }
            else if (expression.Member is PropertyInfo)
            {
                var pi = (PropertyInfo)expression.Member;
                if (pi.GetIndexParameters().Length != 0)
                    throw new ArgumentException("cannot eliminate closure references to indexed properties.");
                value = pi.GetValue(null, null);
            }
            return value;
        }

        private static object EvaluateConstValue(Type type, object value, IList<string> memberNames)
        {
            if (!memberNames.Any())
                return value;

            var memberName = memberNames.First();
            memberNames = memberNames.Skip(1).ToList();

            Type itemType;
            object itemValue;
            var property = type.GetNamedProperty(memberName);
            if (property != null)
            {
                itemType = property.PropertyType;
                itemValue = property.GetValue(value, null);
            }
            else
            {
                var field = type.GetAnyField(memberName, true);
                if (field != null)
                {
                    itemType = field.FieldType;
                    itemValue = field.GetValue(value);
                }
                else
                {
                    return value;
                }
            }

            return EvaluateConstValue(itemType, itemValue, memberNames);
        }
    }
}