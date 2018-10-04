using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    public class CourseSearchService : ICourseSearchService
    {
        private readonly ICourseDbContext _context;

        public CourseSearchService(ICourseDbContext context)
        {
            _context = context;
        }
    }
}
