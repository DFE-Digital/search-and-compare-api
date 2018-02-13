using System.Linq;
using GovUk.Education.SearchAndCompare.Api.Models;
using GovUk.Education.SearchAndCompare.Api.Models.Joins;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.SearchAndCompare.Api.DatabaseAccess
{
    public interface ICourseDbContext
    {

        DbSet<Course> Courses { get; set; }

        DbSet<Subject> Subjects { get; set; }

        DbSet<SubjectArea> SubjectAreas { get; set; }

        DbSet<Campus> Campuses { get; set; }
        
        DbSet<Route> Routes { get; set; }

        DbSet<CourseSubject> CourseSubjects { get; set; }

        IQueryable<Course> GetLocationFilteredCourses(double latitude, double longitude, double radiusInMeters);

        IQueryable<Course> GetCoursesWithProviderAndSubjects();

        IQueryable<Course> GetCoursesWithProviderSubjectsRouteAndCampuses();

        IQueryable<Course> GetCoursesWithProviderSubjectsRouteCampusesAndDescriptions();

        IQueryable<Subject> GetSubjects();

        IQueryable<SubjectArea> GetOrderedSubjectsByArea();
        
        Fees GetLatestFees();
    }
}