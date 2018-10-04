using System.Linq;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    public interface ICourseSearchService
    {
        /// <summary>
        /// Applies pagination and filtering.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        PaginatedList<Course> GetCourses(QueryFilter filter);
    }
}