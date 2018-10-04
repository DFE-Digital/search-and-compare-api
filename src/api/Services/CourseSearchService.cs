using System;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Filters.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    public class CourseSearchService : ICourseSearchService
    {
        private readonly ICourseDbContext _context;

        public CourseSearchService(ICourseDbContext context)
        {
            _context = context;
        }

        public IQueryable<Course> GetFilteredCourses(QueryFilter filter)
        {
            IQueryable<Course> courses;
            bool hasLocationFilter = filter.Coordinates != null && filter.RadiusOption != null;
            bool hasTextFilter = !string.IsNullOrWhiteSpace(filter.query);

            if (hasTextFilter && hasLocationFilter)
            {
                courses = _context.GetTextAndLocationFilteredCourses(
                    filter.query,
                    filter.Coordinates.Latitude,
                    filter.Coordinates.Longitude,
                    filter.RadiusOption.Value.ToMetres());
            }
            else if (hasTextFilter && !hasLocationFilter)
            {
                courses = _context.GetTextFilteredCourses(
                    filter.query);
            }
            else if (!hasTextFilter && hasLocationFilter)
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
    }
}
