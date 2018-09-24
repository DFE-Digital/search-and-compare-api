using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.SearchAndCompare.Api.DatabaseAccess
{
    public interface ICourseDbContext
    {
        DbSet<Course> Courses { get; set; }
        DbSet<Provider> Providers { get; set; }

        DbSet<Subject> Subjects { get; set; }

        DbSet<SubjectArea> SubjectAreas { get; set; }

        DbSet<Campus> Campuses { get; set; }


        DbSet<Route> Routes { get; set; }

        DbSet<FeeCaps> FeeCaps { get; set; }

        DbSet<CourseSubject> CourseSubjects { get; set; }

        DbSet<Contact> Contacts { get; set; }

        DbSet<Location> Locations { get; set; }

        IQueryable<Course> GetLocationFilteredCourses(double latitude, double longitude, double radiusInMeters);

        IQueryable<Course> GetTextFilteredCourses(string searchText);

        IQueryable<Course> GetTextAndLocationFilteredCourses(string searchText, double latitude, double longitude, double radiusInMeters);

        IQueryable<Course> GetCoursesWithProviderSubjectsRouteAndCampuses();

        Task<Course> GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(string providerCode, string courseCode);
        Task AddOrUpdateCourse(Course itemToSave);

        IQueryable<Subject> GetSubjects();

        List<SubjectArea> GetOrderedSubjectsByArea();

        List<FeeCaps> GetFeeCaps();

        List<Provider> SuggestProviders(string query);
        void SaveChanges();

        void Update(Course course);
    }
}