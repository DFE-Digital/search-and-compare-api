using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.Services;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Filters.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Moq;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Unit.Tests.Services
{
    [TestFixture]
    public class CourseSearchServiceTests
    {
        [Test]
        public void CheckFiltering()
        {
            var contextMock = new Mock<ICourseDbContext>();
            var courses = new List<Course>
            {
                new Course
                {
                    Id = 1,
                    Name = "Happy Days",
                    Provider = new Provider
                    {
                        Id = 10,
                        Name = "The Fonz",
                    },
                }
            };
            contextMock.Setup(c =>
                c.GetLocationFilteredCourses(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>())
            ).Returns(courses.AsQueryable());
            ICourseSearchService courseSearchService = new CourseSearchService(contextMock.Object);
            var filter = new QueryFilter
            {
                lat = 1.23,
                lng = 2.34,
                rad = (int)RadiusOption.TenMiles,
            };
            var results = courseSearchService.GetFilteredCourses(filter);
            results.Count.Should().Be(1, "there's one course and location is mocked");
        }
    }
}
