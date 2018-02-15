using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Domain.Lists
{
    public class FilteredList<T> : List<T>
    {
        public int TotalCount { get; private set; }

        public FilteredList(List<T> items, int totalCount)
        {
            TotalCount = totalCount;
            this.AddRange(items);
        }
    }
}
