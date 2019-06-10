using System.Collections.Generic;
using System.Linq;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Api.Helpers;
using GovUk.Education.SearchAndCompare.Domain.Models;
using MockQueryable.Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Unit.Tests
{
    [TestFixture]
    public class CoursesDiffDetailsTests
    {

        public IList<string> GetCourses(int noOfCourses = 1, string programmeCodePrefix = "ProgrammeCode" )
        {
            var result = new List<Course>();
            for (int i = 1; i <= noOfCourses; i++)
            {
                var provider = new Provider() {ProviderCode = "ProviderCode"};
                var course = new Course() {ProgrammeCode =  programmeCodePrefix + i, Provider = provider};
                result.Add(course);
            }

            return result.Select(course => $"[{course.Provider.ProviderCode}, {course.ProgrammeCode}]").OrderBy(x => x).ToList();
        }

        [Test]
        public void Added()
        {
            var currentCourses = GetCourses();
            var recievedCourses = GetCourses(2);

            var diff1 = new CoursesDiffDetails(currentCourses, recievedCourses);

            Assert.That(diff1.Added.FirstOrDefault(x => x.Equals("[ProviderCode, ProgrammeCode2]")), Is.Not.Null);
            Assert.That(diff1.Added.Count == 1, Is.True);
            Assert.That(diff1.Removed.Count == 0, Is.True);
        }

        [Test]
        public void Removed()
        {
            var currentCourses = GetCourses(2);
            var recievedCourses = GetCourses();

            var diff1 = new CoursesDiffDetails(currentCourses, recievedCourses);

            Assert.That(diff1.Removed.FirstOrDefault(x => x.Equals("[ProviderCode, ProgrammeCode2]")), Is.Not.Null);
            Assert.That(diff1.Removed.Count == 1, Is.True);
            Assert.That(diff1.Added.Count == 0, Is.True);
        }

        [Test]
        public void RemovedAndAdded()
        {
            var currentCourses = GetCourses(1, "current");
            var recievedCourses = GetCourses(1, "recieved");

            var diff1 = new CoursesDiffDetails(currentCourses, recievedCourses);

            Assert.That(diff1.Removed.FirstOrDefault(x => x.Equals("[ProviderCode, current1]")), Is.Not.Null);
            Assert.That(diff1.Removed.Count == 1, Is.True);

            Assert.That(diff1.Added.FirstOrDefault(x => x.Equals("[ProviderCode, recieved1]")), Is.Not.Null);
            Assert.That(diff1.Added.Count == 1, Is.True);
        }

        [Test]
        public void NoChange()
        {
            var currentCourses = GetCourses(10);
            var recievedCourses = GetCourses(10);

            var diff1 = new CoursesDiffDetails(currentCourses, recievedCourses);

            Assert.That(diff1.Removed.Count == 0, Is.True);
            Assert.That(diff1.Added.Count == 0, Is.True);
        }
    }
}
