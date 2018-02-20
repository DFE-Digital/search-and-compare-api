using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GovUk.Education.SearchAndCompare.Domain.Lists
{
    public class FilteredList<T> : IFilteredList<T>
    {
        public int TotalCount { get; set; }

        public List<T> Items { get; set; }

        public FilteredList(List<T> items, int totalCount)
        {
            TotalCount = totalCount;
            Items = items;
        }

        // For serialization
        public FilteredList()
        {
        }

        [IgnoreDataMemberAttribute]
        public int Count
        {
            get { return Items.Count; }
        }
    }
}
