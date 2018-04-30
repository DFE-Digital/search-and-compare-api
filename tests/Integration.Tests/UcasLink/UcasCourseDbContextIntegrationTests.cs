using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using GovUk.Education.SearchAndCompare.Api.Controllers;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using GovUk.Education.SearchAndCompare.Api.Integration.Tests.DatabaseAccess;
using Microsoft.Extensions.Options;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.UcasLink
{
    [TestFixture]
    [Category("Integration")]
    [Category("Integration_Ucas")]
    [Explicit]
    public class UcasCourseDbContextIntegrationTests : CourseDbContextIntegrationBase
    {
        [Test]
        public void InsertCourse()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetMinimalCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                var allCourses = context2.Courses.FromSql("SELECT *, NULL as \"Distance\" FROM \"course\"");

                Assert.AreEqual(1, allCourses.Count());
                Assert.AreEqual(GetMinimalCourse().Name, allCourses.First().Name);
            }
        }

        [Test]
        public void UcasController_GetUcasCourseUrl()
        {
            InsertCourse();

            var courseId = context.Courses.FromSql("SELECT *, NULL as \"Distance\" FROM \"course\"").First().Id;

            var ucasSettings = Options.Create<UcasSettings>( new UcasSettings
            {
                GenerateCourseUrlFormat = @"http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/StateId/{3}/HAHTpage/gttr_search.HsProfile.run?inst={0}&course={1}&mod={2}",
                SearchStartUrl = "http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/gttr_search.hjx;start=gttr_search.HsForm.run",
                ExtractStateIdRegex = @"StateId\/([^\/]*)\/"
            });


            var subject = new UcasController(ucasSettings, GetContext(), new HttpClient());

            var actual = subject.GetUcasCourseUrl(courseId).Result as OkObjectResult;

            Assert.IsTrue(actual.StatusCode == 200);

            var expectedCourse = GetMinimalCourse();
            var url = actual.Value.GetType().GetProperty("courseUrl").GetValue(actual.Value, null) as string;
            Assert.NotNull(url);

            StringAssert.Contains(expectedCourse.ProgrammeCode, url);
            StringAssert.Contains(expectedCourse.Provider.ProviderCode, url);

            var stateId = subject.ExtractStateId(url);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(stateId) && url.Length != stateId.Length);
            StringAssert.Contains(stateId, url);
        }

        private static Course GetMinimalCourse()
        {
            return new Course()
            {
                Name = "My minimal course",
                ProgrammeCode = "ProgrammeCode",
                Provider = new Provider
                {
                    Name = "My provider",
                    ProviderCode = "ProviderCode"
                },
                Route = new Route
                {
                    Name = "SCITT"
                },
                Fees = new Fees { Eu = 9250, Uk = 9250, International = 16340 },
            };
        }
    }
}
