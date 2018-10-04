using System;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Filters.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    public class CourseSearchService : ICourseSearchService
    {
        private readonly ICourseDbContext _context;

        public CourseSearchService(ICourseDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IQueryable<Course> GetCourses(QueryFilter filter)
        {
            var courses = GetFilteredCourses(filter);
            courses = ApplySort(filter, courses);
            return courses;
        }

        /// <inheritdoc />
        public IQueryable<Course> AddListingIncludes(IQueryable<Course> queryable)
        {
            return queryable.Include("Provider")
                .Include(course => course.AccreditingProvider)
                .Include(course => course.CourseSubjects)
                .ThenInclude(courseSubject => courseSubject.Subject)
                .ThenInclude(subject => subject.Funding)
                .Include(course => course.ContactDetails)
                .Include(course => course.ProviderLocation)
                .Include(course => course.Route)
                .Include(course => course.Campuses)
                .ThenInclude(campus => campus.Location);
        }

        private IQueryable<Course> GetFilteredCourses(QueryFilter filter)
        {
            bool hasTextFilter = !string.IsNullOrWhiteSpace(filter.query);
            var courses = hasTextFilter ? _context.CoursesByProviderName(filter.query) : _context.GetCoursesWithProviderSubjectsRouteAndCampuses();

            courses = FilterSubjects(filter, courses);
            courses = FilterFunding(filter, courses);
            courses = FilterQualifications(filter, courses);
            courses = FilterParttime(filter, courses);

            // todo: want to aggregate by course, but retain campus distance info
            bool locationFilter = filter.Coordinates != null && filter.RadiusOption != null;
            IQueryable<Location> locations;
            if (locationFilter)
            {
                locations = _context.LocationsNear(
                    filter.Coordinates.Latitude,
                    filter.Coordinates.Longitude,
                    filter.RadiusOption.Value.ToMetres());
            }

            return courses;
        }

        private static IQueryable<Course> FilterSubjects(QueryFilter filter, IQueryable<Course> courses)
        {
            if (filter.SelectedSubjects.Count() > 0)
            {
                courses = courses
                    .Where(course => course.CourseSubjects
                        .Any(courseSubject => filter.SelectedSubjects
                            .Contains(courseSubject.Subject.Id)));
            }

            return courses;
        }

        private static IQueryable<Course> FilterFunding(QueryFilter filter, IQueryable<Course> courses)
        {
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
                                     .Any(courseSubject =>
                                         courseSubject.Subject.FundingId.HasValue &&
                                         courseSubject.Subject.Funding.BursaryFirst.HasValue);
                        break;
                    case FundingOption.NoBursary:
                        f = c => c.IsSalaried || c.CourseSubjects
                                     .Any(courseSubject =>
                                         courseSubject.Subject.FundingId.HasValue &&
                                         courseSubject.Subject.Funding.Scholarship.HasValue);
                        break;
                    case FundingOption.NoSalary:
                        f = c => !c.IsSalaried && c.CourseSubjects
                                     .Any(courseSubject => courseSubject.Subject.FundingId.HasValue);
                        break;
                    case FundingOption.Scholarship:
                        f = c => !c.IsSalaried && c.CourseSubjects
                                     .Any(courseSubject =>
                                         courseSubject.Subject.FundingId.HasValue &&
                                         courseSubject.Subject.Funding.Scholarship.HasValue);
                        break;
                    case FundingOption.Bursary:
                        f = c => !c.IsSalaried && c.CourseSubjects
                                     .Any(courseSubject =>
                                         courseSubject.Subject.FundingId.HasValue &&
                                         courseSubject.Subject.Funding.BursaryFirst.HasValue);
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

            return courses;
        }

        private static IQueryable<Course> FilterQualifications(QueryFilter filter, IQueryable<Course> courses)
        {
            var qualQts = (filter.qualification & (byte)QualificationOption.QtsOnly) > 0;
            var qualPgce = (filter.qualification & (byte)QualificationOption.PgdePgceWithQts) > 0;
            var qualOther = (filter.qualification & (byte)QualificationOption.Other) > 0;

            if (qualQts && qualPgce && qualOther)
            {
                // do nothing - include all qualifications
            }
            else if (qualQts && qualPgce)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.No ||
                                             x.IncludesPgce == IncludesPgce.Yes ||
                                             x.IncludesPgce == IncludesPgce.QtsWithOptionalPgce ||
                                             x.IncludesPgce == IncludesPgce.QtsWithPgde);
            }
            else if (qualQts && qualOther)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.No ||
                                             x.IncludesPgce == IncludesPgce.QtlsOnly ||
                                             x.IncludesPgce == IncludesPgce.QtlsWithPgce ||
                                             x.IncludesPgce == IncludesPgce.QtlsWithPgde);
            }
            else if (qualPgce && qualOther)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.Yes ||
                                             x.IncludesPgce == IncludesPgce.QtsWithOptionalPgce ||
                                             x.IncludesPgce == IncludesPgce.QtsWithPgde ||
                                             x.IncludesPgce == IncludesPgce.QtlsOnly ||
                                             x.IncludesPgce == IncludesPgce.QtlsWithPgce ||
                                             x.IncludesPgce == IncludesPgce.QtlsWithPgde);
            }
            else if (qualQts)
            {
                courses = courses.Where(x => x.IncludesPgce == IncludesPgce.No);
            }
            else if (qualPgce)
            {
                courses = courses.Where(x =>
                    x.IncludesPgce == IncludesPgce.Yes ||
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

            return courses;
        }

        private static IQueryable<Course> FilterParttime(QueryFilter filter, IQueryable<Course> courses)
        {
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

        private static IQueryable<Course> ApplySort(QueryFilter filter, IQueryable<Course> courses)
        {
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
                        // todo
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

            return courses;
        }
    }
}
