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
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GovUk.Education.SearchAndCompare.Geocoder;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.Controllers
{
    [TestFixture]
    public class PubTests : CourseDbContextIntegrationBase
    {
        CoursesController subject;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<CoursesController>>();
            subject = new CoursesController(context, loggerMock.Object, new Mock<IConfiguration>().Object, new Mock<IHttpClient>().Object);
        }


        [Test]
        public void ImportOneCourse_With_existing_locations()
        {
            var pubContext = GetContext();

            pubContext.Locations.RemoveRange();
            pubContext.SaveChanges();

            pubContext.Locations.Add(new Location{Address = "in system Address"});
            pubContext.SaveChanges();
            pubContext.Locations.Count().Should().Be(1);

            var courses = CoursesControllerTests.GetCourses(1);
            var result = subject.Index(courses);

            context.Locations.Count().Should().Be(4);

            for (int i = 1; i <= context.Locations.Count(); i++)
            {
                context.Locations.FirstOrDefault(x => x.Id == i).Should().NotBeNull();
            }

            CoursesControllerTests.AssertOkay(result);
        }
    }
}
