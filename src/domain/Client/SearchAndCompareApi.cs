using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Newtonsoft.Json;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public class SearchAndCompareApi : ISearchAndCompareApi
    {
        private readonly HttpClient _httpClient;

        private readonly string _apiUri;

        public SearchAndCompareApi(HttpClient httpClient, string apiUri)
        {
            _httpClient = httpClient;
            _apiUri = apiUri;
            if (_apiUri.EndsWith('/')) { _apiUri = _apiUri.Remove(_apiUri.Length - 1); }
        }

        public Course GetCourse(int courseId)
        {
            var queryUri = GetUri(string.Format("/courses/{0}", courseId), null);

            return GetObjects<Course>(queryUri);
        }

        public PaginatedList<Course> GetCourses(QueryFilter filter)
        {
            var queryUri = GetUri("/courses", filter);

            return GetObjects<PaginatedList<Course>>(queryUri);
        }

        public List<Subject> GetSubjects()
        {
            var queryUri = GetUri("/subjects", null);

            return GetObjects<List<Subject>>(queryUri);
        }

        public List<SubjectArea> GetSubjectAreas()
        {
            var queryUri = GetUri("/subjectareas", null);

            return GetObjects<List<SubjectArea>>(queryUri);
        }

        public List<FeeCaps> GetFeeCaps()
        {
            var queryUri = GetUri("/feecaps", null);

            return GetObjects<List<FeeCaps>>(queryUri);
        }

        public List<Provider> GetProviderSuggestions(string query)
        {
            var buider = new UriBuilder(new Uri(_apiUri));
            buider.Path += "/providers/suggest";
            buider.Query = "query=" + HttpUtility.UrlEncode(query);
            
            return GetObjects<List<Provider>>(buider.Uri) ?? new List<Provider>();
        }

        public string GetUcasCourseUrl(string programmeCode, string providerCode)
        {
            // todo: get url from api
            return "http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/gttr_search.hjx;start=gttr_search.HsForm.run";
        }

        private T GetObjects<T>(Uri queryUri)
        {
            T objects = default(T);
            var response = _httpClient.GetAsync(queryUri).Result;
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                objects = JsonConvert.DeserializeObject<T>(jsonResponse);
            }
            return objects;
        }

        private Uri GetUri(string apiPath, QueryFilter filter)
        {
            var uri = new Uri(_apiUri);
            var builder = new UriBuilder(uri);
            if (!apiPath.StartsWith('/')) { builder.Path += '/'; }
            builder.Path += apiPath;
            if (filter != null) { builder.Query = filter.AsQueryString(); }
            return builder.Uri;
        }
    }
}