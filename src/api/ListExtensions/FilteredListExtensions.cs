

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.SearchAndCompare.Api.ListExtensions
{
    public static class FilteredListExtensions
    {
        public static FilteredList<T> ToFilteredList<T>(
            this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            var totalCount = source.Count();

            var filtered = source.Where(predicate).ToList();

            return new FilteredList<T>(filtered, totalCount);
        }
    }
}
