using System.Collections.Generic;

namespace GovUk.Education.SearchAndCompare.Domain.Lists
{
    public interface IFilteredList<T> : IList<T>
    {
        int TotalCount { get; }
    }
}