using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Domain.Models;
using MockQueryable.Moq;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Unit
{
    [TestFixture]
    public class FilteredListTests
    {
        [Test]
        public void Create_Subject_Filtered()
        {
            var testFilterIds = new List<int> { 1, 2 };

            var testSubjects = new List<Subject> {
                new Subject { Id = 0 },
                new Subject { Id = 1 },
                new Subject { Id = 2 },
                new Subject { Id = 3 }
            };

            var testSubjectsMock = testSubjects.AsQueryable().BuildMock();

            var filtered = testSubjectsMock.Object.ToFilteredList<Subject>(
                subject => testFilterIds.Contains(subject.Id));

            Assert.That(filtered.Count == 2);
            Assert.That(filtered.TotalCount == 2);
            Assert.That(filtered.Contains(testSubjects[1]));
            Assert.That(filtered.Contains(testSubjects[2]));
        }
    }
}
