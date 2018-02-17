using System.Collections.Generic;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public interface ISearchAndCompareApi
    {
        Course GetCourse(int courseId);

        Fees GetLatestFees();

        List<SubjectArea> GetSubjectAreas();

        PaginatedList<Course> GetCourses(QueryFilter filter);
    }
}