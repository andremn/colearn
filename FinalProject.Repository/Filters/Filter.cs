using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FinalProject.DataAccess.Filters
{
    public class Filter<T> : IFilter<T> where T : class
    {
        private readonly IList<Expression<Func<T, bool>>> _expressions;

        public Filter()
        {
            _expressions = new List<Expression<Func<T, bool>>>();
        }

        public Filter(params Expression<Func<T, bool>>[] expressions)
        {
            _expressions = expressions;
        }

        public ICollection<Expression<Func<T, bool>>> Expressions =>
            _expressions?.ToList() ?? new List<Expression<Func<T, bool>>>();

        public void AddExpression(Expression<Func<T, bool>> expression)
        {
            _expressions.Add(expression);
        }

        public IQueryable<T> Apply(IQueryable<T> queryable)
        {
            return _expressions.Aggregate(queryable, (current, expression) => current.Where(expression));
        }
    }
}