using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GovUk.Education.SearchAndCompare.Api.Middleware
{
    public class BearerTokenApiKeyHandler : AuthenticationHandler<BearerTokenApiKeyOptions>
    {
        private ILogger<BearerTokenApiKeyHandler> _logger;

        public BearerTokenApiKeyHandler(IOptionsMonitor<BearerTokenApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<BearerTokenApiKeyHandler>();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var accessToken = Request.GetAccessToken();

            if (!string.IsNullOrEmpty(accessToken))
            {
                if (accessToken.Equals(Options.ApiKey))
                {
                    var identity = new ClaimsIdentity(
                    new[] {
                            new Claim (ClaimTypes.NameIdentifier, "System")
                    }, BearerTokenApiKeyDefaults.AuthenticationScheme);

                    var princical = new ClaimsPrincipal(identity);

                    var ticket = new AuthenticationTicket(princical, BearerTokenApiKeyDefaults.AuthenticationScheme);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }

                return Task.FromResult(AuthenticateResult.Fail($"Invalid api key: {accessToken}"));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // this method is not async because it's an override!!

            var authResult = HandleAuthenticateOnceSafeAsync().Result;
            var authException = authResult.Failure;
            if (authResult.Succeeded || authException == null)
            {
                return base.HandleChallengeAsync(properties);
            }

            _logger.LogError(authException, "Failed api-key challenge");
            Context.Response.StatusCode = 404; // todo: return 500 if there's an exception, 401 otherwise
            return Task.CompletedTask;

        }
    }
}
