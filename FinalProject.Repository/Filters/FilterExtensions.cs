using System.Linq;
using FinalProject.Shared.Expressions;

namespace FinalProject.DataAccess.Filters
{
    public static class FilterExtensions
    {
        public static IFilter<TTo> ConvertFilter<TFrom, TTo>(this IFilter<TFrom> filter)
            where TTo : class
            where TFrom : class
        {
            var convertedExpressions = filter.Expressions
                .Select(exp => exp.Convert<TFrom, TTo>())
                .ToArray();

            return new Filter<TTo>(convertedExpressions);
        }
    }
}