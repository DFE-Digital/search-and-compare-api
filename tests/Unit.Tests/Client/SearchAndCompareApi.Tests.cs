using System;
using System.Net;
using System.Net.Http;
using System.Text;
using GovUk.Education.SearchAndCompare.Domain.Client;
using Moq;
using NUnit.Framework;

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

            Assert.AreEqual("My first course", result.Name);
            mockHttp.VerifyAll();
        }
                
        [Test]
        public void GetUcasCourseUrl_CallsCorrectUrl()
        {
            mockHttp.Setup(x => x.GetAsync(It.Is<Uri>(y => y.AbsoluteUri == "https://api.example.com/ucas/course-url/XYZ/1AB"))).ReturnsAsync(
                new HttpResponseMessage() {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(@"{""courseUrl"": ""http://ucas.com""}"))
                }
            ).Verifiable();

            var result = sut.GetUcasCourseUrl("XYZ", "1AB");

            Assert.AreEqual("http://ucas.com", result);
            mockHttp.VerifyAll();
        }
    }
}