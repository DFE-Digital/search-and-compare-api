

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
        public static async Task<FilteredList<T>> ToFilteredList<T>(
            this IQueryable<T> source, Expression<Func<T, bool>> predicate)
        {
            var totalCount = await source.CountAsync();

            var filtered = await source.Where(predicate).ToListAsync();

            return new FilteredList<T>(filtered, totalCount);
        }
    }
}
