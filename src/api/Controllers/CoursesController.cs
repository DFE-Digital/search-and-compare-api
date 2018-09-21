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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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
            if(ModelState.IsValid && course != null && course.Provider != null && providerCode == course.Provider.ProviderCode& courseCode == course.ProgrammeCode)
            {
                try
                {
                    IList<Course> courses = new List<Course> {course};
                    Preconditions(courses);
                    MakeProvidersDistinctReferences(ref courses);
                    MakeRoutesDistinctReferences(ref courses);

                    AssociateWithLocations(ref courses);
                    AssociateWithSubjects(ref courses);

                    var existingCourse = await _context.GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(providerCode, courseCode);

                    var itemToSave = courses.First();
                    if(existingCourse == null)
                    {
                        _context.Courses.Add(itemToSave);
                        _logger.LogInformation($"New Course added");

                    }
                    else
                    {
                        Map(existingCourse, ref course);
                        _logger.LogInformation($"Updated existing Course");
                    }


                    _context.SaveChanges();
                    _logger.LogInformation($"Save Course successfully");

                    result = Ok();

                }
                catch(DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "Failed to save the courses to database");
                }
                catch(Exception ex)
                {
                    // Note: notice that ef core dont respect id generation for some reason.
                    _logger.LogWarning(ex, "Failed to save the course");
                }
            }

            return result;
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
            IActionResult result = BadRequest();

            if(ModelState.IsValid && courses != null && courses.Any())
            {
                try
                {
                    _logger.LogInformation($"Courses to import: {courses.Count()}" );
                    Preconditions(courses);
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

                    _context.Courses.AddRange(courses);
                    _context.SaveChanges();

                    result = Ok();
                    _logger.LogInformation($"New Courses Added");
                }
                catch(DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "Failed to save the courses to database");
                }
                catch(Exception ex)
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
            var existingLocations = _context.Locations.ToList();

            var allContactDetailsAddresses = courses.Where(x => !string.IsNullOrWhiteSpace(x.ContactDetails?.Address)).Select(x => x.ContactDetails?.Address) ?? new List<string>();
            var allCampusesAddresses = courses
                .SelectMany(x => x.Campuses.Select(y => y.Location.Address)) ?? new List<string>();
            var allProviderLocationAddresses = courses.Select(x=>x.ProviderLocation?.Address) ?? new List<string>();

            var allAddressesAsLocations = allContactDetailsAddresses
                .Concat(allCampusesAddresses)
                .Concat(allProviderLocationAddresses)
                .Where(x =>  !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToDictionary(x => x, x => {
                    var location = existingLocations.FirstOrDefault(l => l.Address == x);

                    if (location == null) {
                        location = new Location { Address = x };
                        _context.Locations.Add(location);
                    }

                    return location;
                });

            foreach(var course in courses)
            {
                var courseAddress = course.ProviderLocation?.Address ?? course.ContactDetails?.Address;

                if (!string.IsNullOrWhiteSpace(courseAddress))
                {
                    course.ProviderLocation = allAddressesAsLocations[courseAddress];
                }

                foreach (var campus in course.Campuses)
                {
                    var address = campus.Location.Address;
                    campus.Location = allAddressesAsLocations[address];
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

            var distinctSubjects = allSubjects
                .Distinct()
                .Select(x => {
                    var subject = allExistingSubjects.FirstOrDefault(sub => sub.Name == x.Name) ?? x;
                    return subject;})
                .ToLookup(x => x.Name)
                .ToDictionary(x => x.Key, x => x.First());

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
                    .Select(x => x.AccreditingProvider) ??  new List<Provider>();

            var allProviders = coursesProviders.Concat(accreditingProviders);
            var distinctProviders = allProviders
                .Distinct()
                .ToLookup(x => x.ProviderCode)
                .ToDictionary(x => x.Key, x => x.First());


            var existingProviders = _context.Providers.ToList()
                .ToLookup(x => x.ProviderCode)
                .ToDictionary(x => x.Key, x => x.First());;


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

        private void Preconditions(IList<Course> courses)
        {
            var noProvider = courses.Any(x => x.Provider == null || string.IsNullOrWhiteSpace(x.Provider.ProviderCode));
            var noRoute = courses.Any(x => x.Route == null || string.IsNullOrWhiteSpace(x.Route.Name));
            var badSubject = courses.Any(x => {
                var result = false;
                var courseSubjects = (x.CourseSubjects ?? new List<CourseSubject>());

                if(courseSubjects.Count() > 0)
                {
                    result = courseSubjects.Any(cs => cs.Subject == null || string.IsNullOrWhiteSpace(cs.Subject.Name) );
                }

                return result;
            });

            var badAccreditingProvider = courses.Any(x => {
                var result = false;
                if(x.AccreditingProvider != null) {
                    result = string.IsNullOrWhiteSpace(x.AccreditingProvider.ProviderCode);
                }
                return result;
            });

            var badCampus = courses.Any(x => {
                return x.Campuses == null;
            });

            var badFeesOrSalary = courses.Any(x => {
                var result = x.Fees == null && x.Salary == null;

                return result;
            });

            var badProviderLocation = courses.Any(x => x.ProviderLocation == null || string.IsNullOrWhiteSpace(x.ProviderLocation.Address) );

            var badContactDetails = false;//courses.Any(x => {
            //     var cd = x.ContactDetails;
            //     return cd == null;
            // });

            // If this is true then its a no ops, as it will either throw DbUpdateException or InvalidOperationException.
            if(noProvider || noRoute || badSubject || badAccreditingProvider || badCampus || badFeesOrSalary || badProviderLocation || badContactDetails)
            {
                var reason = $"noProvider: {noProvider}, noRoute: {noRoute},  badSubject: {badSubject}, badAccreditingProvider: {badAccreditingProvider}, badCampus: {badCampus}, badFeesOrSalary: {badFeesOrSalary}, badProviderLocation: {badProviderLocation}, badContactDetails: {badContactDetails}";
                throw new InvalidOperationException($"Failed precondition reason: [{reason}] ");
            }
        }
        private static void Map(Course existingCourse, ref Course itemToSave)
        {
            existingCourse.Name = itemToSave.Name;
            existingCourse.ProgrammeCode = itemToSave.ProgrammeCode;
            existingCourse.ProviderCodeName = itemToSave.ProviderCodeName;
            // existingCourse.ProviderId = itemToSave.ProviderId;
            existingCourse.Provider = itemToSave.Provider;
            // existingCourse.AccreditingProviderId = itemToSave.AccreditingProviderId;
            existingCourse.AccreditingProvider = itemToSave.AccreditingProvider;
            existingCourse.AgeRange = itemToSave.AgeRange;
            // existingCourse.RouteId = itemToSave.RouteId;
            existingCourse.Route = itemToSave.Route;
            existingCourse.IncludesPgce = itemToSave.IncludesPgce;
            existingCourse.DescriptionSections = itemToSave.DescriptionSections;
            existingCourse.Campuses = itemToSave.Campuses;
            existingCourse.CourseSubjects = itemToSave.CourseSubjects;
            existingCourse.Fees = itemToSave.Fees;
            existingCourse.IsSalaried = itemToSave.IsSalaried;
            existingCourse.Salary = itemToSave.Salary;
            // existingCourse.ProviderLocationId = itemToSave.ProviderLocationId;
            existingCourse.ProviderLocation = itemToSave.ProviderLocation;
            existingCourse.Distance = itemToSave.Distance;
            // existingCourse.ContactDetailsId = itemToSave.ContactDetailsId;
            existingCourse.ContactDetails = itemToSave.ContactDetails;
            existingCourse.FullTime = itemToSave.FullTime;
            existingCourse.PartTime = itemToSave.PartTime;
            existingCourse.ApplicationsAcceptedFrom = itemToSave.ApplicationsAcceptedFrom;
            existingCourse.StartDate = itemToSave.StartDate;
            existingCourse.Duration = itemToSave.Duration;
            existingCourse.Mod = itemToSave.Mod;
            // itemToSave.Id = existingCourse.Id;
        }
    }
}
