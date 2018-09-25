using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GovUk.Education.SearchAndCompare.Domain.Client;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Moq;
using NUnit.Framework;
using GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.Controllers;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Unit.Tests.Client
{
    [TestFixture]
    public class SearchAndCompareApiTests
    {
        private SearchAndCompareApi sut;
        private Mock<IHttpClient> mockHttp;

        [SetUp]
        public void SetUp()
        {
            mockHttp = new Mock<IHttpClient>();
            sut = new SearchAndCompareApi(mockHttp.Object, "https://api.example.com");
        }

        [Test]
        public void GetCourse_CallsCorrectUrl()
        {
            mockHttp.Setup(x => x.GetAsync(It.Is<Uri>(y => y.AbsoluteUri == "https://api.example.com/courses/XYZ/1AB"))).ReturnsAsync(
                new HttpResponseMessage() {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(@"{""Name"": ""My first course""}"))
                }
            ).Verifiable();

            var result = sut.GetCourse("XYZ", "1AB");

            result.Name.Should().Be("My first course");
            mockHttp.VerifyAll();
        }

        [Test]
        public async Task SaveCourses_CallsCorrectUrl()
        {
            mockHttp.Setup(x => x.PostAsync(It.Is<Uri>(y => y.AbsoluteUri == "https://api.example.com/courses"), It.IsAny<StringContent>())).ReturnsAsync(
                new HttpResponseMessage() {
                    StatusCode = HttpStatusCode.OK
                }
            ).Verifiable();

            var course = CoursesControllerTests.GetCourse(1);
            course.IsValid(false).Should().BeTrue();

            var result = await sut.SaveCoursesAsync(new List<Course>(){course});

            result.Should().BeTrue();
            mockHttp.VerifyAll();
        }

        [Test]
        public async Task SaveCourse_CallsCorrectUrl()
        {
            var course = CoursesControllerTests.GetCourse(1);

            var providerCode = course.Provider.ProviderCode;
            var programmeCode = course.ProgrammeCode;

            mockHttp.Setup(x => x.PostAsync(It.Is<Uri>(y => y.AbsoluteUri == $"https://api.example.com/courses/{providerCode}/{programmeCode}"), It.IsAny<StringContent>())).ReturnsAsync(
                new HttpResponseMessage() {
                    StatusCode = HttpStatusCode.OK
                }
            ).Verifiable();

            course.IsValid(false).Should().BeTrue();
            var result = await sut.SaveCourseAsync(course);

            result.Should().BeTrue();
            mockHttp.VerifyAll();
        }
    }
}
