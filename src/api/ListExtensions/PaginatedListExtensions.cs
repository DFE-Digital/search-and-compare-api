using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.SearchAndCompare.Api.ListExtensions
{
    public static class PaginatedListExtensions
    {
        public static async Task<PaginatedList<T>> ToPaginatedList<T>(
            this IQueryable<T> source, int pageIndex, int pageSize)
        {
            pageIndex = Math.Max(pageIndex, 1);
            pageSize = Math.Max(pageSize, 1);
            var count = await source.CountAsync();
            var totalPages = Math.Max((int)Math.Ceiling(count / (double)pageSize), 1);
            pageIndex = Math.Min(totalPages, pageIndex);

            var items = await source.Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, totalPages, pageIndex, pageSize);
        }
    }
}
