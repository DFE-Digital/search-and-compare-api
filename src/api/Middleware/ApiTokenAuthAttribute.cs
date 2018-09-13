using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.SearchAndCompare.Api.Middleware
{
    public class ApiTokenAuthAttribute : AuthorizeAttribute
    {
        public ApiTokenAuthAttribute()
        {
            AuthenticationSchemes = BearerTokenApiKeyDefaults.AuthenticationScheme;
        }
    }
}
