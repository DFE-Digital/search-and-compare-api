using System.Linq;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    public interface ICourseSearchService
    {
        IQueryable<Course> GetFilteredCourses(QueryFilter filter);
    }
}
