using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.SearchAndCompare.Api.Helpers
{
    public class CircuitBreaker
    {
        private readonly IQueryable<Course> _existingCourses;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public CircuitBreaker(IQueryable<Course> existingCourses, ILogger logger, IConfiguration configuration)
        {
            _existingCourses = existingCourses;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Return true if threshold exceeded so that we can halt the update.
        /// </summary>
        /// <param name="receivedCourses"></param>
        /// <returns>true if threshold exceeded, false if all seems okay</returns>
        public bool Run(ICollection<Course> receivedCourses)
        {
            const string limitKey = "CIRCUIT_BREAKER_COURSE_LIMIT";
            var maxDifference = 100; // default
            var maxDifferenceConfig = _configuration[limitKey];
            if (!string.IsNullOrWhiteSpace(maxDifferenceConfig))
            {
                var valid = int.TryParse(maxDifferenceConfig, out int result);
                if (valid)
                {
                    maxDifference = result;
                }
                else
                {
                    _logger.LogError(
                        $"CircuitBreaker: Non-integer {limitKey} value '{maxDifferenceConfig}', reverting to default value {maxDifference}");
                }
            }

            var currentCourseCount = _existingCourses.Count();
            var receivedCourseCount = receivedCourses?.Count ?? 0;
            var changeInCourseCount = receivedCourseCount - currentCourseCount; // e.g. 100 existing, 98 incoming = difference of -2, i.e. there will be two less courses on find when completed
            var absDiff = Math.Abs(changeInCourseCount);
            var changeInfo = $"Received {absDiff} " + (receivedCourses.Count > currentCourseCount ? "new courses" : "less courses");

            _logger.LogInformation($"CircuitBreaker: [Analysing] Current courses {currentCourseCount},"
                + $" received courses {receivedCourseCount}. {changeInfo}."
                + $" Configured {limitKey}='{maxDifferenceConfig}', active limit: {maxDifference}");

            // circuit breaker for empty post, regardless of configured limits. Once bitten twice shy.
            if (receivedCourses == null || !receivedCourses.Any())
            {
                _logger.LogError( "CircuitBreaker: Tripped! Empty course list received.");
                return true;
            }

            if (currentCourseCount == 0)
            {
                _logger.LogInformation( "CircuitBreaker: Bypassing checks as there are no courses in the database.");
                // This is useful for populating a local test system, or reconstructing production after a fire
                return false;
            }

            var courseInfoFormat = "[{0}, {1}]";

            var recieved = receivedCourses
                .Select(course => string.Format(courseInfoFormat, course.Provider != null ? course.Provider.ProviderCode : null, course.ProgrammeCode))
                .OrderBy(x => x)
                .ToList();
            var current = _existingCourses
                .Select(course => string.Format(courseInfoFormat, course.Provider != null ? course.Provider.ProviderCode : null, course.ProgrammeCode))
                .OrderBy(x => x)
                .ToList();

            var diff = new CoursesDiffDetails(current, recieved);

            var changes = $"Added: {diff.Added.Count()}, Removed: {diff.Removed.Count()}";

            var diffMsgFormat = "CircuitBreaker: Diff {0}: {1}";
            _logger.LogInformation(string.Format(diffMsgFormat, "Summary", changes));

            if(diff.Added.Any())
            {
                _logger.LogInformation(string.Format(diffMsgFormat, "Added", string.Join(",", diff.Added)));
            }

            if(diff.Removed.Any())
            {
                _logger.LogInformation(string.Format(diffMsgFormat, "Removed", string.Join(",", diff.Removed)));
            }

            if (absDiff > maxDifference)
            {
                _logger.LogError($"CircuitBreaker: Tripped! Change exceeded {limitKey}={maxDifference}. {changeInfo}."
                    + $"\nCurrent courses {currentCourseCount}, received courses {receivedCourseCount}.");

                return true;
            }

            // all checks passed
            return false;
        }
    }
}
