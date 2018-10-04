using FluentAssertions;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.Services;
using GovUk.Education.SearchAndCompare.Domain.Filters;
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
            ICourseSearchService courseSearchService = new CourseSearchService(contextMock.Object);
            var filter = new QueryFilter
            {
                pageSize = 123,
            };
            var results = courseSearchService.GetFilteredCourses(filter);
            results.Count.Should().Be(0, "there's no mock data yet");
        }
    }
}
