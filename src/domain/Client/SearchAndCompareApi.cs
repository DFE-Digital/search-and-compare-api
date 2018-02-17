using System.Collections.Generic;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public class CourseDbContext : ISearchAndCompareApi
    {
        public Course GetCourse(int courseId)
        {
            throw new System.NotImplementedException();
        }

        public PaginatedList<Course> GetCourses(QueryFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public Fees GetLatestFees()
        {
            throw new System.NotImplementedException();
        }

        public List<SubjectArea> GetSubjectAreas()
        {
            throw new System.NotImplementedException();
        }
    }
}