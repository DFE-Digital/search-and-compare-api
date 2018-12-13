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
            // Distance and DistanceAddress are not real columns (except when TVF "course_distance" is used)
            modelBuilder.Entity<Course>()
                .Property(x => x.Distance)
                .Metadata.BeforeSaveBehavior = PropertySaveBehavior.Ignore;

            modelBuilder.Entity<Course>()
                .Property(x => x.Distance)
                .Metadata.AfterSaveBehavior = PropertySaveBehavior.Ignore;

            modelBuilder.Entity<Course>()
                .Property(x => x.DistanceAddress)
                .Metadata.BeforeSaveBehavior = PropertySaveBehavior.Ignore;

            modelBuilder.Entity<Course>()
                .Property(x => x.DistanceAddress)
                .Metadata.AfterSaveBehavior = PropertySaveBehavior.Ignore;

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

            //cascade delete between Course Campuses
            modelBuilder.Entity<Campus>()
                .HasOne(p => p.Course)
                .WithMany(b => b.Campuses)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DefaultCourseDescriptionSection>();

            base.OnModelCreating(modelBuilder);
        }


        public void Update(Course course)
        {
            base.Update(course);
        }
        public new void SaveChanges()
        {
            base.SaveChanges();
        }

        public IQueryable<Course> GetLocationFilteredCourses(double latitude, double longitude, double radiusInMeters)
        {
            return ForListing(Courses.FromSql(@"
SELECT ""course"".*, distance.""Distance"", distance.""DistanceAddress""
FROM course_distance(@lat,@lon,@rad) AS distance
JOIN ""course"" ON ""course"".""Id"" = ""distance"".""Id""",
                    new NpgsqlParameter("@lat", latitude),
                    new NpgsqlParameter("@lon", longitude),
                    new NpgsqlParameter("@rad", radiusInMeters)));

        }

        public IQueryable<Course> GetTextFilteredCourses(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Cannot be null or white space", nameof(searchText));
            }

            return ForListing(Courses.FromSql(@"
SELECT ""course"".*, NULL as ""Distance"", NULL as ""DistanceAddress""
FROM ""course""
LEFT OUTER JOIN ""provider"" AS ""p1"" ON ""course"".""ProviderId"" = ""p1"".""Id""
LEFT OUTER JOIN ""provider"" AS ""p2"" ON ""course"".""AccreditingProviderId"" = ""p2"".""Id""
WHERE lower(""p1"".""Name"") = lower(@query) OR lower(""p2"".""Name"") = lower(@query)",
                    new NpgsqlParameter("@query", searchText)));
        }

        public IQueryable<Course> GetTextAndLocationFilteredCourses(string searchText, double latitude, double longitude, double radiusInMeters)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Cannot be null or white space", nameof(searchText));
            }

            return ForListing(Courses.FromSql(@"
SELECT ""course"".*, c1.""Distance"", c1.""DistanceAddress""
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
                .Include(x => x.DescriptionSections).FirstOrDefaultAsync();
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

            #pragma warning disable EF1000

            return Courses.FromSql(
                "SELECT \"course\".*, NULL as \"Distance\", NULL as \"DistanceAddress\" FROM \"course\" " +
                "LEFT OUTER JOIN \"provider\" ON \"course\".\"ProviderId\" = \"provider\".\"Id\"" +
                whereClause,
                sqlParams.ToArray());

            #pragma warning restore EF1000
        }

        private IQueryable<Course> GetCoursesWithProviderSubjectsRouteAndCampuses(string providerCode, string courseCode)
        {
            return ForListing(GetCourses(providerCode, courseCode));
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
    OR (to_tsvector('english', ""provider"".""ProviderCode"") @@ to_tsquery('english', quote_literal(@query) || ':*')) IS TRUE
    GROUP BY ""provider"".""Id"") AS sub
ORDER BY lower(""Name"") <> lower(@query) ASC, ""cnt"" DESC
LIMIT @limit",
            new NpgsqlParameter("@query", query),
            new NpgsqlParameter("@limit", 5))
            .ToList();
        }

        private IQueryable<Course> ForListing(IQueryable<Course> queryable)
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
            existingCourse.HasVacancies = itemToSave.HasVacancies;
            existingCourse.IsSen = itemToSave.IsSen;
            // itemToSave.Id = existingCourse.Id;
        }
    }
}
