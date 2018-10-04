using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.Middleware;
using GovUk.Education.SearchAndCompare.Api.Services;
using GovUk.Education.SearchAndCompare.Domain.Data;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class CoursesController : Controller
    {
        private readonly ICourseDbContext _context;

        private readonly ILogger _logger;
        private readonly ICourseSearchService _courseSearchService;

        public CoursesController(ICourseDbContext courseDbContext, ICourseSearchService courseSearchService, ILogger<CoursesController> logger)
        {
            _context = courseDbContext;
            _logger = logger;
            _courseSearchService = courseSearchService;
        }

        [HttpPost("{providerCode}/{courseCode}")]
        [ApiTokenAuth]
        public async Task<IActionResult> SaveCourse(string providerCode, string courseCode, [FromBody]Course course)
        {
            //
            // TODO:
            //   Match up subjects to an exisiting list (currently pulled from the list of distinct subjects in the importer)
            //   This includes matching those existing subjects to a subject-area and to subject-funding.
            //
            IActionResult result = BadRequest();

            if (ModelState.IsValid &&
                course != null &&
                course.IsValid(false) &&
                providerCode.Equals(course.Provider.ProviderCode, StringComparison.InvariantCultureIgnoreCase) &&
                courseCode.Equals(course.ProgrammeCode, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    IList<Course> courses = new List<Course> { course };
                    MakeProvidersDistinctReferences(ref courses);
                    MakeRoutesDistinctReferences(ref courses);

                    AssociateWithLocations(ref courses);
                    AssociateWithSubjects(ref courses);

                    ResolveSalaryAndFeesReferences(ref courses);
                    var itemToSave = courses.First();

                    await _context.AddOrUpdateCourse(itemToSave);

                    _context.SaveChanges();
                    _logger.LogInformation($"Added/Updated Course successfully");

                    result = Ok();

                }
                // Note: if any exception is catch it course.IsValid() needs to be revisited
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "Failed to save the courses to database");
                }
                catch (Exception ex)
                {
                    // Note: notice that ef core dont respect id generation for some reason.
                    _logger.LogWarning(ex, "Failed to save the course");
                }
            }

            return result;
        }

        [HttpPost]
        [ApiTokenAuth]
        [RequestSizeLimit(100_000_000_000)]
        public IActionResult Index([FromBody]IList<Course> courses)
        {
            //
            // TODO:
            //   Match up subjects to an exisiting list (currently pulled from the list of distinct subjects in the importer)
            //   This includes matching those existing subjects to a subject-area and to subject-funding.
            //
            IActionResult result = BadRequest();

            if (ModelState.IsValid &&
                courses != null &&
                courses.All(x => x.IsValid(false)))
            {
                try
                {
                    _logger.LogInformation($"Courses to import: {courses.Count()}");
                    _context.Campuses.RemoveRange(_context.Campuses);
                    _context.Courses.RemoveRange(_context.GetCoursesWithProviderSubjectsRouteAndCampuses());
                    _context.Providers.RemoveRange(_context.Providers);
                    _context.Contacts.RemoveRange(_context.Contacts);
                    _context.Routes.RemoveRange(_context.Routes);

                    _context.SaveChanges();
                    _logger.LogInformation($"Existing Courses Removed");

                    MakeProvidersDistinctReferences(ref courses);
                    MakeRoutesDistinctReferences(ref courses);
                    AssociateWithLocations(ref courses);
                    AssociateWithSubjects(ref courses);
                    ResolveSalaryAndFeesReferences(ref courses);

                    _context.Courses.AddRange(courses);
                    _context.SaveChanges();

                    result = Ok();
                    _logger.LogInformation($"New Courses Added");
                }
                // Note: if any exception is catch it course.IsValid() needs to be revisited
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "Failed to save the courses to database");
                }
                catch (Exception ex)
                {
                    // Note: notice that ef core dont respect id generation for some reason.
                    _logger.LogWarning(ex, "Failed to save the courses");
                }
            }

            return result;
        }

        [HttpGet("total")]
        public IActionResult GetCoursesTotal(QueryFilter filter)
        {
            var totalCount = _courseSearchService.GetFilteredCourseCount(filter);

            return Ok(new TotalCountResult { TotalCount = totalCount });
        }

        // GET api/courses
        [HttpGet]
        public IActionResult GetFiltered(QueryFilter filter)
        {
            var paginatedCourses = _courseSearchService.GetFilteredCourses(filter);
            return Ok(paginatedCourses);
        }

        // GET api/courses/{id}
        [HttpGet("{providerCode}/{courseCode}")]
        public async Task<IActionResult> GetByCourseCode(string providerCode, string courseCode)
        {
            var course = await _context.GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(providerCode, courseCode);

            return Ok(course);
        }

        private void AssociateWithLocations(ref IList<Course> courses)
        {
            var existingLocations = _context.Locations.ToList();

            var allContactDetailsAddresses = courses.Where(x => !string.IsNullOrWhiteSpace(x.ContactDetails?.Address)).Select(x => x.ContactDetails?.Address) ?? new List<string>();
            var allCampusesAddresses = courses
                .SelectMany(x => x.Campuses.Select(y => y.Location.Address)) ?? new List<string>();
            var allProviderLocationAddresses = courses.Select(x => x.ProviderLocation?.Address) ?? new List<string>();

            var allAddressesAsLocations = allContactDetailsAddresses
                .Concat(allCampusesAddresses)
                .Concat(allProviderLocationAddresses)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToDictionary(x => x, x =>
                {
                    var location = existingLocations.FirstOrDefault(l => l.Address == x);

                    if (location == null)
                    {
                        location = new Location { Address = x };
                    }

                    return location;
                });

            foreach (var course in courses)
            {
                var courseAddress = course.ProviderLocation?.Address ?? course.ContactDetails?.Address;

                if (!string.IsNullOrWhiteSpace(courseAddress))
                {
                    course.ProviderLocation = allAddressesAsLocations[courseAddress];
                }
                else
                {
                    course.ProviderLocation = null;
                }

                foreach (var campus in course.Campuses)
                {
                    var address = campus.Location.Address;
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        campus.Location = allAddressesAsLocations[address];
                    }
                    else
                    {
                        campus.Location = null;
                    }
                }
            }
        }

        private void AssociateWithSubjects(ref IList<Course> courses)
        {
            var allSubjectCourses = courses
                .SelectMany(x => x.CourseSubjects)
                .Where(cs => cs.Subject != null) ?? new List<CourseSubject>();

            var allSubjects = allSubjectCourses
                    .Select(y => y.Subject) ??
                new List<Subject>();

            var allExistingSubjects = _context.Subjects.ToList();

            var defaultAreaOrNull = _context.SubjectAreas.FirstOrDefault(x => x.Name == "Secondary")
                ?? _context.SubjectAreas.FirstOrDefault();

            var distinctSubjects = allSubjects
                .ToLookup(x => x.Name)
                .ToDictionary(x => x.Key, x =>
                {
                    var subject = allExistingSubjects.FirstOrDefault(s => s.Name == x.Key);

                    if (subject == null)
                    {
                        subject = new Subject { Name = x.Key, SubjectArea = defaultAreaOrNull };
                    }

                    return subject;
                });

            foreach (var c in courses)
            {
                foreach (var cSub in c.CourseSubjects)
                {
                    var subjectName = cSub.Subject.Name;
                    cSub.Subject = distinctSubjects[subjectName];
                }
            }
        }

        private void MakeProvidersDistinctReferences(ref IList<Course> courses)
        {
            var coursesProviders = courses
                    .Select(x => x.Provider) ?? new List<Provider>();
            var accreditingProviders = courses.Where(x => x.AccreditingProvider != null)
                    .Select(x => x.AccreditingProvider) ?? new List<Provider>();

            var allProviders = coursesProviders.Concat(accreditingProviders);
            var distinctProviders = allProviders
                .Distinct()
                .ToLookup(x => x.ProviderCode)
                .ToDictionary(x => x.Key, x => x.First());


            var existingProviders = _context.Providers.ToList()
                .ToLookup(x => x.ProviderCode)
                .ToDictionary(x => x.Key, x => x.First()); ;


            foreach (var course in courses)
            {
                if (course.AccreditingProvider != null)
                {
                    course.AccreditingProvider = existingProviders.GetValueOrDefault(course.AccreditingProvider.ProviderCode) ?? distinctProviders[course.AccreditingProvider.ProviderCode];
                }
                course.Provider = existingProviders.GetValueOrDefault(course.Provider.ProviderCode) ?? distinctProviders[course.Provider.ProviderCode];
            }
        }

        private void MakeRoutesDistinctReferences(ref IList<Course> courses)
        {
            var existing = _context.Routes.ToList()
                .ToLookup(x => x.Name)
                .ToDictionary(x => x.Key, x => x.First());

            var distinctRoutes = courses.Select(x => x.Route)
                .Distinct()
                .ToLookup(x => x.Name)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var course in courses)
            {
                if (course.Route != null)
                {
                    course.Route = existing.GetValueOrDefault(course.Route.Name) ?? distinctRoutes[course.Route.Name];
                }
            }
        }

        private void ResolveSalaryAndFeesReferences(ref IList<Course> courses)
        {
            // Even if there is no salary create a new instance to satisfy ef core.
            // The 'course' table is shared with 'Fees' &'Salary' & 'Course' domain object
            foreach (var course in courses)
            {
                course.Fees = course.Fees ?? new Fees();
                course.Salary = course.Salary ?? new Salary();
            }
        }
    }
}
