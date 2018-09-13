using System;
using Microsoft.AspNetCore.Authentication;

namespace GovUk.Education.SearchAndCompare.Api.Middleware
{
    public static class BearerTokenApiKeyExtensions
    {
        public static AuthenticationBuilder AddBearerTokenApiKey(this AuthenticationBuilder builder, Action<BearerTokenApiKeyOptions> bearerTokenApiKeyOptions)
        {
            return builder.AddScheme<BearerTokenApiKeyOptions, BearerTokenApiKeyHandler>(BearerTokenApiKeyDefaults.AuthenticationScheme, BearerTokenApiKeyDefaults.AuthenticationDisplayName, bearerTokenApiKeyOptions);
        }
    }
}
