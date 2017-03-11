using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FinalProject.Shared.Expressions.Visitors
{
    public class TypeConversionVisitor : ExpressionVisitor
    {
        private readonly Dictionary<Expression, Expression> _parameterMap;

        public TypeConversionVisitor(
            Dictionary<Expression, Expression> parameterMap)
        {
            _parameterMap = parameterMap;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression found;

            if (!_parameterMap.TryGetValue(node, out found))
            {
                found = base.VisitParameter(node);
            }

            return found;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expr = Visit(node.Expression);

            if (expr.Type == node.Type)
            {
                return base.VisitMember(node);
            }

            var newMember = expr.Type.GetMember(node.Member.Name)
                .Single();

            return Expression.MakeMemberAccess(expr, newMember);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var parameters = node.Parameters
                .Select(Visit)
                .Cast<ParameterExpression>()
                .ToArray();

            return Expression.Lambda(Visit(node.Body), parameters);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arguments = Visit(node.Arguments);
            var genericArguments = node.Method.GetGenericArguments();

            if (genericArguments.Length == 0)
            {
                return base.VisitMethodCall(node);
            }

            var newGenericArguments = genericArguments
                .Select(Map)
                .ToArray();

            var method = node.Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(newGenericArguments);

            return Expression.Call(node.Object, method, arguments);
        }

        private static Type Map(Type type)
        {
            var mappedType = TypeMapper.Map(type);

            return mappedType ?? type;
        }
    }
}
