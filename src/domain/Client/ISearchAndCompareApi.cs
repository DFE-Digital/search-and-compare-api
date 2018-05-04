using System.Collections.Generic;
using GovUk.Education.SearchAndCompare.Domain.Data;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public interface ISearchAndCompareApi
    {
        Course GetCourse(int courseId);

        PaginatedList<Course> GetCourses(QueryFilter filter);
        TotalCountResult GetCoursesTotalCount(QueryFilter filter);

        List<Subject> GetSubjects();

        List<SubjectArea> GetSubjectAreas();

        List<FeeCaps> GetFeeCaps();

        List<Provider> GetProviderSuggestions(string query);

        string GetUcasCourseUrl(int courseId);
    }
}
