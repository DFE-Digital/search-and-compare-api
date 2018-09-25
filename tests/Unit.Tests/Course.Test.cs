using System.Collections.Generic;
using System.Linq;
using GovUk.Education.SearchAndCompare.Domain.Models;
using MockQueryable.Moq;
using NUnit.Framework;
using FluentAssertions;
using System;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Unit.Tests
{
    [TestFixture]
    public class CourseTest
    {
        [Test]
        public void IsValid_false()
        {
            var course = new Course();
            course.IsValid().Should().BeFalse();
        }

        [Test]
        public void IsValid_false_exception()
        {
            var course = new Course();
            Action act = () => course.IsValid(true);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
