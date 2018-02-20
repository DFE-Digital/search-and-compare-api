using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GovUk.Education.SearchAndCompare.Domain.Lists
{
    public class PaginatedList<T> : IPaginatedList<T>
    {
        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public List<T> Items { get; set; }

        public PaginatedList(List<T> items, int totalCount, int totalPages, int pageIndex, int pageSize)
        {
            TotalCount = totalCount;
            TotalPages = totalPages;
            PageIndex = pageIndex;
            PageSize = pageSize;
            Items = items;
        }

        // For serialization
        public PaginatedList()
        {
        }

        [IgnoreDataMemberAttribute]
        public int Count
        {
            get { return Items.Count; }
        }

        [IgnoreDataMemberAttribute]
        public bool HasPreviousPage
        {
            get { return (PageIndex > 1); }
        }

        [IgnoreDataMemberAttribute]
        public bool HasNextPage
        {
            get { return (PageIndex < TotalPages); }
        }
    }
}
