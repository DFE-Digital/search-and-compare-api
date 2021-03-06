using System.Threading.Tasks;
using System.Collections.Generic;
using GovUk.Education.SearchAndCompare.Domain.Data;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public interface ISearchAndCompareApi
    {
        Course GetCourse(string providerCode, string courseCode);

        Task<bool> UpdateCoursesAsync(IList<Course> courses);

        Task<bool> SaveCoursesAsync(IList<Course> courses);

        PaginatedList<Course> GetCourses(QueryFilter filter);
        TotalCountResult GetCoursesTotalCount(QueryFilter filter);

        List<Subject> GetSubjects();

        List<SubjectArea> GetSubjectAreas();

        List<FeeCaps> GetFeeCaps();

        List<Provider> GetProviderSuggestions(string query);
    }
}
