using System.Collections.Generic;

namespace GovUk.Education.SearchAndCompare.Domain.Lists
{
    public interface IPaginatedList<T>
    {
        int TotalCount { get; set; }

        int TotalPages { get; set; }

        int PageIndex { get; set; }

        int PageSize { get; set; }

        List<T> Items { get; set; }

        bool HasPreviousPage { get; }

        bool HasNextPage { get; }
    }
}