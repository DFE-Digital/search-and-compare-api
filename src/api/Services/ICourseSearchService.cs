using System.Linq;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    public interface ICourseSearchService
    {
        /// <summary>
        /// Retrieves course list from database based on filters supplied.
        /// Does not apply pagination, that is up to the consumer of this method's output.
        /// </summary>
        IQueryable<Course> GetCourses(QueryFilter filter);
    }
}
