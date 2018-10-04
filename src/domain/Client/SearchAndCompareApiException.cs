using System;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public class SearchAndCompareApiException : Exception
    {
        public SearchAndCompareApiException(string message) : base(message)
        {
        }

        public SearchAndCompareApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}