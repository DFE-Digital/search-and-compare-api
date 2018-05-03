using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Api.Ucas
{
    public interface IUcasUrlBuilder
    {
        /// <summary>
        /// Get a url with a new sessionId from the UCAS website.
        /// Note this involves making a GET request to the UCAS site.
        /// </summary>
        Task<string> GenerateCourseUrl(string providerCode, string programmeCode, string modifier);
    }
}
