using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    public interface ICourseSearchService
    {
        PaginatedList<Course> GetFilteredCourses(QueryFilter filter);
        int GetFilteredCourseCount(QueryFilter filter);
    }
}
