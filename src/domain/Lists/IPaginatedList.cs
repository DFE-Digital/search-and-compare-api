using System.Collections.Generic;

namespace GovUk.Education.SearchAndCompare.Domain.Lists
{
    public interface IPaginatedList<T> : IList<T>
    {
        int PageIndex { get; }

        int TotalPages { get; }

        int TotalCount { get; }

        bool HasPreviousPage { get; }

        bool HasNextPage { get; }
    }
}