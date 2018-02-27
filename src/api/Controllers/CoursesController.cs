using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Filters.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class CoursesController : Controller
    {
        private readonly ICourseDbContext _context;

        private int pageSize = 10;

        public CoursesController(ICourseDbContext courseDbContext)
        {
            _context = courseDbContext;
        }

        // GET api/courses
        [HttpGet]
        public IActionResult GetFiltered(QueryFilter filter)
        {
            IQueryable<Course> courses;
            if (filter.Coordinates != null && filter.RadiusOption != null)
            {
                courses = _context.GetLocationFilteredCourses(
                    filter.Coordinates.Latitude,
                    filter.Coordinates.Longitude,
                    filter.RadiusOption.Value.ToMetres());
            } 
            else
            {
                courses = _context.GetCoursesWithProviderSubjectsRouteAndCampuses(); 
            }

            if (filter.SelectedSubjects.Count() > 0)
            {
                courses = courses
                    .Where(course => course.CourseSubjects
                        .Any(courseSubject => filter.SelectedSubjects
                            .Contains(courseSubject.Subject.Id)));
            }

            if (filter.SelectedFunding != FundingOption.All)
            {
                if (filter.SelectedFunding == FundingOption.BursaryOrScholarship)
                {
                    courses = courses
                        .Where(course => course.Subjects
                            .Any(subject => subject.FundingId.HasValue));
                }
                else if (filter.SelectedFunding == FundingOption.Salary)
                {
                    courses = courses
                        .Where(course => course.IsSalaried);
                }
            }

            switch (filter.SortBy)
            {
                case (SortByOption.ZtoA):
                {
                    courses = courses.OrderByDescending(c => c.Provider.Name);
                    break;
                }
                case (SortByOption.Distance):
                {
                    courses = courses.OrderBy(c => c.Distance);
                    break;
                }
                default:
                case (SortByOption.AtoZ):
                {
                    courses = courses.OrderBy(c => c.Provider.Name);
                    break;
                }
            }

            var paginatedCourses = courses.ToPaginatedList<Course>(filter.page ?? 1, pageSize);

            return Ok(paginatedCourses);
        }

        // GET api/courses/{id}
        [HttpGet("{courseId:int}")]
        public async Task<IActionResult> GetById(int courseId)
        {
            var course = await _context.GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(courseId);

            return Ok(course);
        }
    }
}
