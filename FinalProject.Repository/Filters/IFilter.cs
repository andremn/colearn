using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FinalProject.DataAccess.Filters
{
    public interface IFilter<TObject> where TObject : class
    {
        ICollection<Expression<Func<TObject, bool>>> Expressions { get; }

        void AddExpression(Expression<Func<TObject, bool>> expression);

        IQueryable<TObject> Apply(IQueryable<TObject> queryable);
    }
}