using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GovUk.Education.SearchAndCompare.Api.Helpers;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Unit.Tests
{
    [TestFixture]
    public class CircuitBreakerTests
    {
        private IQueryable<Course> _existingCourses;
        private Mock<ILogger> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private List<Course> _receivedCourses;

        [SetUp]
        public void Setup()
        {
            _existingCourses = new List<Course>().AsQueryable();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration = new Mock<IConfiguration>();
            _receivedCourses = new List<Course>();
        }

        [Test]
        public void LogsCourseCounts()
        {
            // Arrange
            _receivedCourses = new List<Course>
            {
                new Course(),
                new Course(),
            };
            _existingCourses = new List<Course>
            {
                new Course(),
                new Course(),
                new Course(),
            }.AsQueryable();

            // Act
            RunCircuitBreaker();

            // Assert
            AssertLog(LogLevel.Information,
                $"Circuit breaker: Current courses {_existingCourses.Count()}, received courses {_receivedCourses.Count}.");
        }

        [Test]
        public void EmptyPayloadTrips()
        {
            // Arrange
            _receivedCourses = new List<Course>();

            // Act
            var tripped = RunCircuitBreaker();

            // Assert
            tripped.Should().Be(true, "because no courses in payload");
            AssertLog(LogLevel.Error,
                "CircuitBreakerTripped: empty course list received");
        }

        [Test]
        public void BypassWhenDatabaseEmpty()
        {
            // Arrange
            _existingCourses = new List<Course>().AsQueryable();
            _receivedCourses = new List<Course> { new Course() };

            // Act
            var tripped = RunCircuitBreaker();

            // Assert
            tripped.Should().Be(false, "because database is empty so nothing to protect");
            AssertLog(LogLevel.Information,
                "CircuitBreaker: bypassing checks as there are no courses in the database.");
        }

        [Test]
        public void BypassWhenNotConfigured()
        {
            // Arrange
            _existingCourses = new List<Course> { new Course() }.AsQueryable();
            _receivedCourses = new List<Course> { new Course() };

            // Act
            var tripped = RunCircuitBreaker();

            // Assert
            tripped.Should().Be(false, "because unconfigured");
            AssertLog(LogLevel.Warning,
                "CircuitBreaker: bypassing checks as no limit configured. Configure with: CIRCUIT_BREAKER_COURSE_LIMIT");
        }

        [Test]
        public void TripsWhenLimitExceeded()
        {
            // Arrange
            _existingCourses = new List<Course>
            {
                new Course(), new Course(), new Course(), new Course(), new Course(),
            }.AsQueryable();
            _receivedCourses = new List<Course>
            {
                new Course(), new Course(),
            };
            SetLimit("2");

            // Act
            var tripped = RunCircuitBreaker();

            // Assert
            tripped.Should().Be(true, "too many courses removed");
            AssertLog(LogLevel.Error,
                "CircuitBreakerTripped: Change exceeded CIRCUIT_BREAKER_COURSE_LIMIT=2. Received 3 less courses.\n"
                +"Current courses 5, received courses 2.");
        }

        [Test]
        public void PassesWhenWithinLimit()
        {
            // Arrange
            _existingCourses = new List<Course>
            {
                new Course(), new Course(), new Course(), new Course(), new Course(),
            }.AsQueryable();
            _receivedCourses = new List<Course>
            {
                new Course(), new Course(), new Course(),
            };
            SetLimit("2");

            // Act
            var tripped = RunCircuitBreaker();

            // Assert
            tripped.Should().Be(false, "diff is within configured limit");
        }

        private bool RunCircuitBreaker()
        {
            var circuitBreaker = new CircuitBreaker(_existingCourses, _mockLogger.Object, _mockConfiguration.Object);
            return circuitBreaker.Run(_receivedCourses);
        }

        private void SetLimit(string limit)
        {
            _mockConfiguration.SetupGet(p => p[It.Is<string>(s => s == "CIRCUIT_BREAKER_COURSE_LIMIT")])
                .Returns(limit);
        }

        private void AssertLog(LogLevel logLevel, string expectedMessage)
        {
            // can't mock the LoggerExtensions so mock the ILogger.Log() that they delegate to instead
            // ref: https://stackoverflow.com/questions/43424095/how-to-unit-test-with-ilogger-in-asp-net-core/56728528#56728528
            _mockLogger.Verify(x =>
                x.Log(logLevel,
                    0,
                    It.Is<FormattedLogValues>(formattedLogValues => messageMatches(expectedMessage, formattedLogValues)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()
                ));
        }

        private static bool messageMatches(string expectedMessage, FormattedLogValues formattedLogValues)
        {
            // separate method as opposed to lambda for easier debugging

            return formattedLogValues.ToString() == expectedMessage;
        }
    }
}
