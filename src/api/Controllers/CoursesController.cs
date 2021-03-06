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
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using GovUk.Education.SearchAndCompare.Domain.Client;
using GovUk.Education.SearchAndCompare.Geocoder;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GovUk.Education.SearchAndCompare.Api.Helpers;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class CoursesController : Controller
    {
        private readonly ICourseDbContext _context;

        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly Geocoder.ILocationRequester _locationRequester;
        private int defaultPageSize = 10;

        public CoursesController(ICourseDbContext courseDbContext, ILogger<CoursesController> logger, IConfiguration configuration, ILocationRequester locationRequester)
        {
            _context = courseDbContext;
            _logger = logger;

            _locationRequester = locationRequester;
            _configuration = configuration;
        }

        /// <summary>
        /// Add/update one or more courses.
        /// This is used when a user on manage courses publishes a course, or publish a provider's courses.
        /// </summary>
        [HttpPut("")]
        [ApiTokenAuth]
        [RequestSizeLimit(100_000_000_000)]
        public async Task<IActionResult> SaveCourses([FromBody]IList<Course> courses)
        {
            IActionResult result = BadRequest();

            if (courses == null)
            {
                return result;
            }

            var allCoursesValid = courses.All(x => x != null && x.IsValid(false));

            if (ModelState.IsValid && allCoursesValid)
            {
                try
                {
                    MakeProvidersDistinctReferences(ref courses);
                    MakeRoutesDistinctReferences(ref courses);

                    AssociateWithLocations(ref courses);
                    AssociateWithSubjects(ref courses);

                    ResolveSalaryAndFeesReferences(ref courses);

                    foreach (var course in courses)
                    {
                        await _context.AddOrUpdateCourse(course);
                    }

                    _context.SaveChanges();
                    _logger.LogInformation($"Added/Updated Course successfully");

                    var exitcode = await _locationRequester.RequestLocations();
                    if (exitcode != 0)
                    {
                        _logger.LogWarning($"Failed to geocode {exitcode} location");
                    }

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

        /// <summary>
        /// Wipe database and replace the contents with what was posted.
        /// This is used by the scheduled bulk export from manage courses
        /// into this system.
        /// </summary>
        [HttpPost]
        [ApiTokenAuth]
        [RequestSizeLimit(100_000_000_000)]
        public IActionResult Index([FromBody]IList<Course> courses)
        {
            if (CircuitBreakerTripped(courses))
            {
                return BadRequest();
            }

            //
            // TODO:
            //   Match up subjects to an exisiting list (currently pulled from the list of distinct subjects in the importer)
            //   This includes matching those existing subjects to a subject-area and to subject-funding.
            //

            IActionResult result = BadRequest();
            if (ModelState.IsValid &&
                courses.All(x => x.IsValid(false)))
            {
                var success = false;
                // Strategy is required for compatibility with EnableRetryOnFailure in Startup. Ref https://docs.microsoft.com/en-gb/azure/architecture/best-practices/retry-service-specific#sql-database-using-entity-framework-core
                var strategy = ((DbContext)_context).Database.CreateExecutionStrategy();
                strategy.Execute(() =>
                {
                    using (var transaction = (_context as DbContext).Database.BeginTransaction())
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
                            _logger.LogInformation($"Existing Courses Removed (don't worry it's in a transaction, they'll be back soon)");

                            MakeProvidersDistinctReferences(ref courses);
                            MakeRoutesDistinctReferences(ref courses);
                            AssociateWithLocations(ref courses);
                            AssociateWithSubjects(ref courses);
                            ResolveSalaryAndFeesReferences(ref courses);

                            _context.Courses.AddRange(courses);
                            _context.SaveChanges();
                            transaction.Commit();

                            result = Ok();
                            success = true;
                            _logger.LogInformation($"New Courses Added");
                        }
                        // Note: if any exception is catch it course.IsValid() needs to be revisited
                        catch (DbUpdateException ex)
                        {
                            _logger.LogWarning(ex, "Failed to save the courses to database");
                            transaction.Rollback();
                        }
                        catch (Exception ex)
                        {
                            // Note: notice that ef core dont respect id generation for some reason.
                            _logger.LogWarning(ex, "Failed to save the courses");
                            transaction.Rollback();
                        }
                    }
                });

                if (success)
                {
                    // a complete re-import of the data messes up the table statistics
                    // and leads to the query optimiser choosing prohibitively slow execution plans
                    // Do a stats rebuild befroe finishing.
                    // Run synchronously (as ASP.NET cancels async commands that outlive the Controller Action).
                    (_context as DbContext).Database.ExecuteSqlCommand("VACUUM ANALYZE");
                }
            }

            return result;
        }

        /// <summary>
        /// Return true if threshold exceeded so that we can halt the update.
        /// </summary>
        /// <param name="receivedCourses"></param>
        /// <returns>true if threshold exceeded, false if all seems okay</returns>
        private bool CircuitBreakerTripped(ICollection<Course> receivedCourses)
        {
            var circuitBreaker = new CircuitBreaker(_context.Courses, _logger, _configuration);
            return circuitBreaker.Run(receivedCourses);
        }

        [HttpGet("total")]
        public IActionResult GetCoursesTotal(QueryFilter filter)
        {
            var totalCount = GetFilteredCourses(filter).Count();

            return Ok(new TotalCountResult { TotalCount = totalCount });
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
                        courses = courses
                            .OrderBy(c => c.Provider.Name != filter.query) // false comes before true... (odd huh)
                            .ThenByDescending(c => c.Provider.Name)
                            .ThenBy(c => c.Name);
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
                        courses = courses
                            .OrderBy(c => c.Provider.Name != filter.query) // false comes before true... (odd huh)
                            .ThenBy(c => c.Provider.Name)
                            .ThenBy(c => c.Name);
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

            if (course == null)
            {
                return NotFound();
            }

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
                Expression<Func<Course, bool>> f;
                switch (filter.SelectedFunding)
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

            var qualQts = (filter.qualification & (byte)QualificationOption.QtsOnly) > 0;
            var qualPgce = (filter.qualification & (byte)QualificationOption.PgdePgceWithQts) > 0;
            var qualOther = (filter.qualification & (byte)QualificationOption.Other) > 0;

            if (qualQts && qualPgce && qualOther)
            {
                // do nothing - include all qualifications
            }
            else if (qualQts && qualPgce)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.QtsOnly ||
                    x.IncludesPgce == IncludesPgce.QtsWithPgce ||
                    x.IncludesPgce == IncludesPgce.QtsWithOptionalPgce ||
                    x.IncludesPgce == IncludesPgce.QtsWithPgde);
            }
            else if (qualQts && qualOther)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.QtsOnly ||
                    x.IncludesPgce == IncludesPgce.QtlsOnly ||
                    x.IncludesPgce == IncludesPgce.QtlsWithPgce ||
                    x.IncludesPgce == IncludesPgce.QtlsWithPgde);
            }
            else if (qualPgce && qualOther)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.QtsWithPgce ||
                    x.IncludesPgce == IncludesPgce.QtsWithOptionalPgce ||
                    x.IncludesPgce == IncludesPgce.QtsWithPgde ||
                    x.IncludesPgce == IncludesPgce.QtlsOnly ||
                    x.IncludesPgce == IncludesPgce.QtlsWithPgce ||
                    x.IncludesPgce == IncludesPgce.QtlsWithPgde);
            }
            else if (qualQts)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.QtsOnly);
            }
            else if (qualPgce)
            {
                courses = courses.Where(x =>
                    x.IncludesPgce == IncludesPgce.QtsWithPgce ||
                    x.IncludesPgce == IncludesPgce.QtsWithOptionalPgce ||
                    x.IncludesPgce == IncludesPgce.QtsWithPgde);
            }
            else if (qualOther)
            {
                courses = courses.Where(x =>
                    x.IncludesPgce == IncludesPgce.QtlsOnly ||
                    x.IncludesPgce == IncludesPgce.QtlsWithPgce ||
                    x.IncludesPgce == IncludesPgce.QtlsWithPgde);
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

            if (filter.hasvacanciesonly)
            {
                courses = courses.Where(course => course.HasVacancies);
            }

            if (filter.senCourses)
            {
                courses = courses.Where(c => c.IsSen);
            }

            return courses;
        }

        private void AssociateWithLocations(ref IList<Course> courses)
        {
            var allContactDetailsAddresses = courses.Where(x => !string.IsNullOrWhiteSpace(x.ContactDetails?.Address)).Select(x => x.ContactDetails?.Address) ?? new List<string>();

            var existingLocations = _context.Locations.ToList();

            var allCampusLocations = courses
                .SelectMany(x => x.Campuses
                    .Where(c => !string.IsNullOrWhiteSpace(c.Name) && !string.IsNullOrWhiteSpace(c.Location?.Address) )
                    .Select(c => {
                        var result = c.Location.Address;

                        if(!c.Name.Equals("main site", StringComparison.InvariantCultureIgnoreCase))
                        {
                            result = $"{c.Name}, {c.Location.Address}";
                        }

                        return new KeyValuePair<string, string>(result, c.Location.Address );
                    })).GroupBy(x => x.Key).Select(g => g.First()).ToDictionary(x => x.Key, x =>
                {
                    var location = existingLocations.FirstOrDefault(l => l.GeoAddress == x.Key);
                    if (location == null)
                    {
                        location = new Location { Address = x.Value, GeoAddress =x.Key };

                        var old = existingLocations.FirstOrDefault(l => l.Address == x.Value);

                        if(old != null){
                            location.Latitude = old.Latitude;
                            location.Longitude = old.Longitude;
                        }
                    }

                    return location;
                }) ?? new Dictionary<string, Location>();

            var allProviderLocationAddresses = courses.Select(x => x.ProviderLocation?.Address) ?? new List<string>();


            var allAddressesAsLocations = allContactDetailsAddresses
                .Concat(allProviderLocationAddresses)
                .Where(x => !string.IsNullOrWhiteSpace(x) && !allCampusLocations.ContainsKey(x) )
                .Distinct()
                .ToDictionary(x => x, x =>
                {
                    var location = existingLocations.FirstOrDefault(l => l.GeoAddress == x);
                    if (location == null)
                    {
                        location = new Location { Address = x, GeoAddress =x };
                    }

                    return location;
                })
                .Concat(allCampusLocations)
                .ToDictionary(x => x.Key, x=> x.Value);

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
                    var address = campus.Location?.Address;

                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        if(!campus.Name.Equals("main site", StringComparison.InvariantCultureIgnoreCase))
                        {
                            address = $"{campus.Name}, {address}";
                        }

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
                .ToDictionary(x => x.Key, x => x.First());

            // update provider names
            foreach (var provider in existingProviders.Values)
            {
                provider.Name = distinctProviders.TryGetValue(provider.ProviderCode, out Provider newProvider)
                    ? newProvider.Name
                    : provider.Name;
            }


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
