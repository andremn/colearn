using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FinalProject.Shared.Expressions.Visitors;

namespace FinalProject.Shared.Expressions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<TTo, bool>> Convert<TFrom, TTo>(
           this Expression<Func<TFrom, bool>> from)
        {
            return ConvertExpression<Func<TFrom, bool>, Func<TTo, bool>>(from);
        }

        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second,
            Func<Expression, Expression, Expression> merge)
        {
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        public static Expression<Func<T, bool>> And<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            if (expressions == null)
            {
                throw new ArgumentNullException(nameof(expressions));
            }

            return expressions.Aggregate((left, right) => left.Compose(right, Expression.And));
        }

        public static Expression<Func<T, bool>> Or<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            if (expressions == null)
            {
                throw new ArgumentNullException(nameof(expressions));
            }

            return expressions.Aggregate((left, right) => left.Compose(right, Expression.Or));
        }

        private class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

            private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map,
                Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;

                if (_map.TryGetValue(p, out replacement))
                {
                    p = replacement;
                }

                return base.VisitParameter(p);
            }
        }

        private static Expression<TTo> ConvertExpression<TFrom, TTo>(Expression<TFrom> from)
            where TFrom : class
            where TTo : class
        {
            return (Expression<TTo>)ConvertLambdaExpression(from).Body;
        }

        private static LambdaExpression ConvertLambdaExpression(Expression from)
        {
            var parameterMap = new Dictionary<Expression, Expression>();
            var parameters = MapParameters(from, parameterMap);

            var body = new TypeConversionVisitor(parameterMap).Visit(from);

            return Expression.Lambda(body, parameters);
        }

        private static IEnumerable<ParameterExpression> MapParameters(
            Expression expression,
            IDictionary<Expression, Expression> parameterMap)
        {
            var parameters = new List<ParameterExpression>();
            var newParameters = new List<ParameterExpression>();
            var lambdaExpression = expression as LambdaExpression;
            var memberExpression = expression as MemberExpression;
            var methodCallExpression = expression as MethodCallExpression;
            var unaryExpression = expression as UnaryExpression;
            var binaryExpression = expression as BinaryExpression;

            if (lambdaExpression != null)
            {
                parameters.AddRange(lambdaExpression.Parameters);
            }

            if (methodCallExpression != null)
            {
                foreach (var argument in methodCallExpression.Arguments)
                {
                    MapParameters(argument, parameterMap);
                }
            }

            if (unaryExpression != null)
            {
                MapParameters(unaryExpression.Operand, parameterMap);
            }

            if (binaryExpression != null)
            {
                MapParameters(binaryExpression.Left, parameterMap);
                MapParameters(binaryExpression.Right, parameterMap);
            }

            var parameterExpression = memberExpression?.Expression as ParameterExpression;

            if (parameterExpression != null)
            {
                parameters.Add(parameterExpression);
            }

            foreach (var parameter in parameters)
            {
                var newType = TypeMapper.Map(parameter.Type);

                if (newType == null)
                {
                    newParameters.Add(parameter);
                    continue;
                }

                var newParameterExpression = Expression.Parameter(newType, parameter.Name);
                
                parameterMap[parameter] = newParameterExpression;
                newParameters.Add(newParameterExpression);
            }

            if (lambdaExpression?.Body != null)
            {
                MapParameters(lambdaExpression.Body, parameterMap);
            }

            return newParameters;
        }
    }
}
