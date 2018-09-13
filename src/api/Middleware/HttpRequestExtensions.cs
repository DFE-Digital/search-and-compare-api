using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ManageCourses.Api.Middleware
{
    public static class HttpRequestExtensions
    {
        public static string GetAccessToken(this HttpRequest request)
        {
            var authorization = "Authorization";
            var authorizationHeaderValues = request.Headers.ContainsKey(authorization) ?
                ((string)request.Headers[authorization]).Split(' ') : new[] { "", "" };

            var accessToken = authorizationHeaderValues.Length == 2 && authorizationHeaderValues[0].ToLowerInvariant().Equals("bearer") ?
                authorizationHeaderValues[1] : "";

            return accessToken;
        }
    }
}
