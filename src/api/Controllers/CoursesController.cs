using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Api.Middleware;
using GovUk.Education.SearchAndCompare.Domain.Data;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Filters.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class CoursesController : Controller
    {
        private readonly ICourseDbContext _context;

        private readonly ILogger _logger;
        private int defaultPageSize = 10;

        public CoursesController(ICourseDbContext courseDbContext, ILogger<CoursesController> logger)
        {
            _context = courseDbContext;
            _logger = logger;
        }

        [HttpPost]
        [ApiTokenAuth]
        public IActionResult Index([FromBody]IList<Course> courses)
        {
            //
            // TODO:
            //   Match up subjects to an exisiting list (currently pulled from the list of distinct subjects in the importer)
            //   This includes matching those existing subjects to a subject-area and to subject-funding.
            //
            if(ModelState.IsValid && courses != null && courses.Any())
            {
                try
                {
                    _context.Campuses.RemoveRange(_context.Campuses);
                    _context.Courses.RemoveRange(_context.GetCoursesWithProviderSubjectsRouteAndCampuses());
                    _context.Providers.RemoveRange(_context.Providers);
                    _context.Contacts.RemoveRange(_context.Contacts);
                    _context.Routes.RemoveRange(_context.Routes);
                    _context.Subjects.RemoveRange(_context.Subjects); // todo: remove this when subjects are reference data

                    _context.SaveChanges();

                    MakeProvidersDistinctReferences(ref courses);
                    MakeRoutesDistinctReferences(ref courses);
                    MakeSubjectsDistinctReferences(ref courses); // todo: change when subjects are reference data

                    AssociateWithLocations(ref courses);

                    _context.Courses.AddRange(courses);
                    _context.SaveChanges();

                    return Ok();
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to save the course");

                    return BadRequest();
                }
            }
            else{
                return BadRequest();
            }
        }

        [HttpGet("total")]
        public IActionResult GetCoursesTotal(QueryFilter filter)
        {
            var totalCount = GetFilteredCourses(filter).Count();

            return Ok(new TotalCountResult{TotalCount= totalCount });
        }

        // GET api/courses
        [HttpGet]
        public IActionResult GetFiltered(QueryFilter filter)
        {
            var courses = GetFilteredCourses(filter);

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

            var pageSize = defaultPageSize;

            if (filter.pageSize.HasValue)
            {
                if (filter.pageSize.Value == 0)
                {
                    pageSize = int.MaxValue;
                }
                else
                {
                    pageSize = filter.pageSize.Value;
                }
            }

            var paginatedCourses = courses.ToPaginatedList<Course>(filter.page ?? 1, pageSize);

            return Ok(paginatedCourses);
        }

        // GET api/courses/{id}
        [HttpGet("{providerCode}/{courseCode}")]
        public async Task<IActionResult> GetByCourseCode(string providerCode, string courseCode)
        {
            var course = await _context.GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(providerCode, courseCode);

            return Ok(course);
        }

        private IQueryable<Course> GetFilteredCourses(QueryFilter filter)
        {
            IQueryable<Course> courses;
            bool locationFilter = filter.Coordinates != null && filter.RadiusOption != null;
            bool textFilter = !string.IsNullOrWhiteSpace(filter.query);

            if (textFilter && locationFilter)
            {
                courses = _context.GetTextAndLocationFilteredCourses(
                    filter.query,
                    filter.Coordinates.Latitude,
                    filter.Coordinates.Longitude,
                    filter.RadiusOption.Value.ToMetres());
            }
            else if (textFilter && !locationFilter)
            {
                courses = _context.GetTextFilteredCourses(
                    filter.query);
            }
            else if (!textFilter && locationFilter)
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
                Expression<Func<Course,bool>> f;
                switch(filter.SelectedFunding)
                {
                    case FundingOption.AnyFunding:
                        f = c => c.IsSalaried || c.CourseSubjects
                            .Any(courseSubject => courseSubject.Subject.FundingId.HasValue);
                        break;
                    case FundingOption.NoScholarship:
                        f = c => c.IsSalaried || c.CourseSubjects
                            .Any(courseSubject => courseSubject.Subject.FundingId.HasValue && courseSubject.Subject.Funding.BursaryFirst.HasValue);
                        break;
                    case FundingOption.NoBursary:
                        f = c => c.IsSalaried || c.CourseSubjects
                            .Any(courseSubject => courseSubject.Subject.FundingId.HasValue && courseSubject.Subject.Funding.Scholarship.HasValue);
                        break;
                    case FundingOption.NoSalary:
                        f = c => !c.IsSalaried && c.CourseSubjects
                            .Any(courseSubject => courseSubject.Subject.FundingId.HasValue);
                        break;
                    case FundingOption.Scholarship:
                        f = c => !c.IsSalaried && c.CourseSubjects
                            .Any(courseSubject => courseSubject.Subject.FundingId.HasValue && courseSubject.Subject.Funding.Scholarship.HasValue);
                        break;
                    case FundingOption.Bursary:
                        f = c => !c.IsSalaried && c.CourseSubjects
                            .Any(courseSubject => courseSubject.Subject.FundingId.HasValue && courseSubject.Subject.Funding.BursaryFirst.HasValue);
                        break;
                    case FundingOption.Salary:
                        f = c => c.IsSalaried;
                        break;
                    default:
                        f = null;
                        break;
                }

                if (f != null)
                {
                    courses = courses.Where(f);
                }
            }

            if (!filter.pgce ^ !filter.qts)
            {
                if (!filter.pgce)
                {
                    courses = courses.Where(course => course.IncludesPgce != IncludesPgce.Yes);
                }
                else if (!filter.qts)
                {
                    courses = courses.Where(course => course.IncludesPgce != IncludesPgce.No);
                }
            }

            if (!filter.parttime ^ !filter.fulltime)
            {
                if (!filter.parttime)
                {
                    courses = courses.Where(course => course.FullTime != VacancyStatus.NA);
                }
                else if (!filter.fulltime)
                {
                    courses = courses.Where(course => course.PartTime != VacancyStatus.NA);
                }
            }

            return courses;
        }

        private void AssociateWithLocations(ref IList<Course> courses)
        {
            var allAddressesAsLocations = new List<string>()
                .Concat(courses.Select(x => x.ContactDetails?.Address))
                .Concat(courses.SelectMany(x => x.Campuses.Select(y => y.Location?.Address)))
                .Concat(courses.Select(x=>x.ProviderLocation?.Address))
                .Where(x =>  !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToDictionary(x => x, x => new Location { Address = x });

            var allExistingLocations = _context.Locations.ToList()
                .ToLookup(x => x.Address)
                .ToDictionary(x => x.Key, x => x.First());

            foreach(var course in courses)
            {
                var courseAddress = course.ProviderLocation?.Address ?? course.ContactDetails?.Address;

                if (!string.IsNullOrWhiteSpace(courseAddress))
                {
                    course.ProviderLocation = allExistingLocations.TryGetValue(courseAddress, out Location existing)
                        ? existing
                        : allAddressesAsLocations[course.ProviderLocation.Address];
                }

                foreach (var campus in course.Campuses)
                {
                    if(!string.IsNullOrWhiteSpace(campus.Location?.Address))
                    {
                        campus.Location = allExistingLocations.TryGetValue(campus.Location?.Address, out Location existing)
                        ? existing
                        : allAddressesAsLocations[campus.Location?.Address];
                    }
                }
            }
        }

        private static void MakeProvidersDistinctReferences(ref IList<Course> courses)
        {
            var distinctProviders = courses.Where(x => x.Provider != null).Select(x => x.Provider)
                .Concat(courses.Where(x => x.AccreditingProvider != null).Select(x => x.AccreditingProvider))
                .Distinct()
                .ToLookup(x => x.ProviderCode)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var course in courses)
            {
                if (course.AccreditingProvider != null)
                {
                    course.AccreditingProvider = distinctProviders[course.AccreditingProvider.ProviderCode];
                }

                if(course.Provider != null)
                {
                    course.Provider = distinctProviders[course.Provider.ProviderCode];
                }
            }
        }

        private static void MakeRoutesDistinctReferences(ref IList<Course> courses)
        {
            var distinctRoutes = courses.Select(x => x.Route)
                .Distinct()
                .ToLookup(x => x.Name)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var course in courses)
            {
                if (course.Route != null)
                {
                    course.Route = distinctRoutes[course.Route.Name];
                }
            }
        }

        private void MakeSubjectsDistinctReferences(ref IList<Course> courses)
        {
            var distinctSubjects = courses.SelectMany(x => x.CourseSubjects.Select(y => y.Subject))
                .Distinct()
                .ToLookup(x => x.Name)
                .ToDictionary(x => x.Key, x => x.First());

            // Hardcore it "Secondary" else whatever first
            var subjectArea = _context.SubjectAreas.FirstOrDefault(x => x.Name == "Secondary") ?? _context.SubjectAreas.FirstOrDefault();

            var courseSubjects = courses.SelectMany(x => x.CourseSubjects)
                .Select(x => {

                    x.Subject.SubjectArea = x.Subject.SubjectArea ?? subjectArea;

                    return x;
                    }
                )
                .ToList();

            foreach (var courseSubject in courseSubjects)
            {
                courseSubject.Subject = distinctSubjects[courseSubject.Subject.Name];
            }
        }
    }
}
