using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace GovUk.Education.SearchAndCompare.Api.DatabaseAccess
{
    public class CourseDbContext : DbContext, ICourseDbContext
    {
        public DbSet<Course> Courses { get; set; }

        public DbSet<Provider> Providers { get; set; }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<SubjectArea> SubjectAreas { get; set; }

        public DbSet<Campus> Campuses { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Route> Routes { get; set; }

        public DbSet<FeeCaps> FeeCaps { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        // Join tables
        public DbSet<CourseSubject> CourseSubjects { get; set; }

        public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Distance is not a real column (except when TVF "course_distance" is used)
            modelBuilder.Entity<Location>()
                .Property(x => x.Distance)
                .Metadata.BeforeSaveBehavior = PropertySaveBehavior.Ignore;

            modelBuilder.Entity<Course>().OwnsOne(p => p.Fees);

            modelBuilder.Entity<Course>().OwnsOne(p => p.Salary);

            // Location Index
            modelBuilder.Entity<Location>()
                .HasIndex(b => new { b.Longitude, b.Latitude });

            // Many-Many mapping between Course and Subject
            modelBuilder.Entity<CourseSubject>()
                .HasKey(c => new { c.CourseId, c.SubjectId });

            modelBuilder.Entity<CourseSubject>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseSubjects)
                .HasForeignKey(cs => cs.CourseId);

            modelBuilder.Entity<CourseSubject>()
                .HasOne(cs => cs.Subject)
                .WithMany(c => c.CourseSubjects)
                .HasForeignKey(cs => cs.SubjectId);

            modelBuilder.Entity<DefaultCourseDescriptionSection>();

            base.OnModelCreating(modelBuilder);
        }


        public void Update(Course course)
        {
            base.Update(course);
        }
        public void SaveChanges()
        {
            base.SaveChanges();
        }

        /// <summary>
        /// Filters to courses that are an exact match on provider / accrediting provider name
        /// </summary>
        public IQueryable<Course> CoursesByProviderName(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Cannot be null or white space", nameof(providerName));
            }

            return Courses.FromSql(@"
SELECT ""course"".*, NULL as ""Distance""
FROM ""course""
LEFT OUTER JOIN ""provider"" AS ""p1"" ON ""course"".""ProviderId"" = ""p1"".""Id""
LEFT OUTER JOIN ""provider"" AS ""p2"" ON ""course"".""AccreditingProviderId"" = ""p2"".""Id""
WHERE lower(""p1"".""Name"") = lower(@query) OR lower(""p2"".""Name"") = lower(@query)",
                new NpgsqlParameter("@query", providerName));
        }

        public IQueryable<Location> LocationsNear(double latitude, double longitude, double radiusInMeters)
        {
            // This code and supporting functions was written with the following needs in mind:
            // Search results needs a list of courses, sorted by distance, within a radius;
            // where distance is from the user's search location (e.g. postcode) to the
            // location of either provider or a campus.
            // We need to show the address of the provider if that was nearest,
            // or the name & address of the campus if that was nearest, along with the fact it's a campus.
            // Need to be able to filter arbitrarily on any other course/campus info.
            // The code needs to be easy to iterate on as we learn what users actually need from location search.

            // Data structure being created:
            // List of locations that match distance criteria, sorted nearest first.
            // For each location there is either a course or a campus attached to it.
            // If it's a campus then the course can be retrieved by navigating that relationship.

            // filtered locations
            // join
            // filtered courses
            // filtered courses via campus
            // filtered campuses

            // e.g. locations near x (course or campus), sorted by distance
            // but only computer science courses
            // and only those with a salary
            // and maybe in the future a restriction on campuses in some form

            // how to use:
            // get queryable of location by lat/lon distance
            // get queryable of course, apply filters
            // get queryable of campus, apply filters
            // join them all together
            // apply distance sort
            // execute (tolist)

            var locationsWithDistance = Locations.FromSql(@"
                    SELECT *
                    FROM location_distance(@lat,@lat,@radius) AS loc
                    join location on location.""Id"" = loc.""Id""
                ", latitude, longitude, radiusInMeters);

            return locationsWithDistance;
        }

        public IQueryable<Course> GetTextAndLocationFilteredCourses(string searchText, double latitude, double longitude, double radiusInMeters)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Cannot be null or white space", nameof(searchText));
            }

            return Courses.FromSql(@"
SELECT ""course"".*, c1.""Distance""
FROM course_distance(@lat,@lon,@rad) AS ""c1""
JOIN ""course"" on ""course"".""Id"" = ""c1"".""Id""
LEFT OUTER JOIN ""provider"" AS ""p1"" ON ""course"".""ProviderId"" = ""p1"".""Id""
LEFT OUTER JOIN ""provider"" AS ""p2"" ON ""course"".""AccreditingProviderId"" = ""p2"".""Id""
WHERE lower(""p1"".""Name"") = lower(@query) OR lower(""p2"".""Name"") = lower(@query)",
                    new NpgsqlParameter("@query", searchText),
                    new NpgsqlParameter("@lat", latitude),
                    new NpgsqlParameter("@lon", longitude),
                    new NpgsqlParameter("@rad", radiusInMeters)));
        }

        public IQueryable<Course> GetCoursesWithProviderSubjectsRouteAndCampuses()
        {
            return GetCoursesWithProviderSubjectsRouteAndCampuses(null, null);
        }

        public async Task AddOrUpdateCourse(Course itemToSave)
        {
            var existingCourse = await GetCourses(itemToSave.Provider.ProviderCode, itemToSave.ProgrammeCode)
            // Note: added course subjects as lack of it causes ef core tracking issues (ie added source and update with same data cause it to add course subject rather than update it)
            .Include(x => x.CourseSubjects)
            .Include(x => x.DescriptionSections)
            .Include(x => x.Campuses)
            .FirstOrDefaultAsync();

            if(existingCourse == null)
            {
                itemToSave.Id = 0;

                Courses.Add(itemToSave);
            }
            else
            {
                Map(existingCourse, itemToSave);
            }
        }

        public async Task<Course> GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(string providerCode, string courseCode)
        {
            return await GetCoursesWithProviderSubjectsRouteAndCampuses(providerCode, courseCode)
                .Include(x => x.DescriptionSections).FirstAsync();
        }

        private IQueryable<Course> GetCourses(string providerCode, string courseCode)
        {
            var sqlParams = new List<NpgsqlParameter>();
            var whereClauses = new List<string>();


            if (!string.IsNullOrWhiteSpace(courseCode))
            {
                whereClauses.Add("lower(\"course\".\"ProgrammeCode\") = lower(@coursecode)");
                sqlParams.Add(new NpgsqlParameter("@coursecode", courseCode));
            }

            if (!string.IsNullOrWhiteSpace(providerCode))
            {
                whereClauses.Add("lower(\"provider\".\"ProviderCode\") = lower(@providercode)");
                sqlParams.Add(new NpgsqlParameter("@providercode", providerCode));
            }

            var whereClause = whereClauses.Any()
                ? " WHERE " + string.Join(" AND ", whereClauses)
                : "";

            return Courses.FromSql(
                "SELECT \"course\".*, NULL as \"Distance\" FROM \"course\" " +
                "LEFT OUTER JOIN \"provider\" ON \"course\".\"ProviderId\" = \"provider\".\"Id\"" +
                whereClause,
                sqlParams.ToArray());
        }

        private IQueryable<Course> GetCoursesWithProviderSubjectsRouteAndCampuses(string providerCode, string courseCode)
        {
            return GetCourses(providerCode, courseCode);
        }

        public IQueryable<Subject> GetSubjects()
        {
            return from subject in Subjects select subject;
        }

        public List<SubjectArea> GetOrderedSubjectsByArea()
        {
            return SubjectAreas
                .Include(x => x.Subjects)
                    .ThenInclude(y => y.Funding)
                .OrderBy(x => x.Ordinal)
                .AsNoTracking()
                .ToList();
        }

        public List<FeeCaps> GetFeeCaps()
        {
            return FeeCaps.ToList();
        }

        public List<Provider> SuggestProviders(string query)
        {
            return Providers.FromSql(@"
SELECT * FROM (
    SELECT ""provider"".*, COUNT(*) AS cnt
    FROM ""provider""
    JOIN ""course"" ON ""course"".""ProviderId"" = ""provider"".""Id""  OR ""course"".""AccreditingProviderId"" = ""provider"".""Id""
    WHERE (to_tsvector('english', ""provider"".""Name"") @@ to_tsquery('english', quote_literal(@query) || ':*')) IS TRUE
    GROUP BY ""provider"".""Id"") AS sub
ORDER BY ""cnt"" DESC
LIMIT @limit",
            new NpgsqlParameter("@query", query),
            new NpgsqlParameter("@limit", 5))
            .ToList();
        }

        private static void Map(Course existingCourse, Course itemToSave)
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
