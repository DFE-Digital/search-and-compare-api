using System;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;

namespace GovUk.Education.SearchAndCompare.Api.Middleware
{
    public class BearerTokenApiKeyOptions : AuthenticationSchemeOptions
    {
        public string ApiKey { get; set; }

        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ApiKey)) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The '{0}' option must be provided.", nameof(this.ApiKey)), nameof(this.ApiKey));

            }

            base.Validate();
        }
    }
}
