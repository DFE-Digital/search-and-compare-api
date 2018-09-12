using System.Collections.Generic;
using System.Linq;
using GovUk.Education.SearchAndCompare.Api.Controllers;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using NUnit;
using GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.DatabaseAccess;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.Controllers
{
    [TestFixture]
    public class CoursesControllerTests : CourseDbContextIntegrationBase
    {

        CoursesController subject;

        [SetUp]
        public void Setup() 
        {
            subject = new CoursesController(context);
        }

        [Test]
        public void ImportOneCourse() 
        {
            var courses = GetCourses(1);
            var result = subject.Index(courses);

            Assert.IsNotNull(result);
        }

        [Test]
        public void ImportTwoCourse() 
        {
            var courses = GetCourses(2);
            var result = subject.Index(courses);

            var resultingCourses = context.GetCoursesWithProviderSubjectsRouteAndCampuses().ToList();
            var resultingProviders = context.Providers.ToList();

            Assert.AreEqual(1, resultingProviders.Count);
            Assert.AreEqual(2, resultingCourses.Count);
            Assert.True(object.ReferenceEquals(resultingCourses[0].Provider, resultingCourses[1].Provider));
        }

        [Test]
        public void ImportTwice()
        {
            var result1 = subject.Index(GetCourses(20));
            var result2 = subject.Index(GetCourses(2));

            var resultingCourses = context.GetCoursesWithProviderSubjectsRouteAndCampuses().ToList();
            var resultingProviders = context.Providers.ToList();

            Assert.AreEqual(1, resultingProviders.Count);
            Assert.AreEqual(2, resultingCourses.Count);
            Assert.True(object.ReferenceEquals(resultingCourses[0].Provider, resultingCourses[1].Provider));
        }

        private List<Course> GetCourses(int count)
        {
            var courses = new List<Course>();

            for (int i = 0; i < count; i++)
            {
                var course = new Course 
                {
                    Name = "Name" + i,
                    ProviderId = 42,
                    Provider = new Provider {
                        Id = 24,
                        Name = "Name",
                        ProviderCode = "ProviderCode"
                    },
                    AgeRange = AgeRange.Secondary,
                    Route = new Route 
                    {
                        Name = "Scitt" + i,
                        IsSalaried = true
                    },

                    IncludesPgce = IncludesPgce.No,
                    Fees = new Fees (),
                    Salary = new Salary (),
                    ContactDetails = new Contact(),
                };

                courses.Add(course);
            }

            return courses;
        }
    }
}