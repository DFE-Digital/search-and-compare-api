using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using GovUk.Education.SearchAndCompare.Domain.Data;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Lists;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public class SearchAndCompareApi : ISearchAndCompareApi
    {
        private readonly IHttpClient _httpClient;

        private readonly string _apiUri;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public SearchAndCompareApi(HttpClient httpClient, string apiUri) : this (new HttpClientWrapper(httpClient), apiUri)
        {
        }

        public SearchAndCompareApi(IHttpClient httpClient, string apiUri)
        {
            if(string.IsNullOrWhiteSpace(apiUri))
            {
                throw new SearchAndCompareApiException($"Failed to instantiate due apiUri is null or white space");
            }

            _httpClient = httpClient;
            _apiUri = apiUri;
            if (_apiUri.EndsWith('/')) { _apiUri = _apiUri.Remove(_apiUri.Length - 1); }
        }

        public Course GetCourse(string providerCode, string courseCode)
        {
            var queryUri = GetUri($"/courses/{providerCode}/{courseCode}");

            return GetObjects<Course>(queryUri);
        }

        public async Task<bool> UpdateCoursesAsync(IList<Course> courses)
        {
            var result = courses.All(course => course.IsValid(false));

            if (result)
            {
                var queryUri = GetUri($"/courses");

                var coursesJson = JsonConvert.SerializeObject(courses, _serializerSettings);

                var courseStringContent = new StringContent(coursesJson, Encoding.UTF8, "application/json" );
                var response = await _httpClient.PutAsync(queryUri, courseStringContent);

                result = response.IsSuccessStatusCode;
            }

            return result;
        }

        public async Task<bool> SaveCoursesAsync(IList<Course> courses)
        {
            var result = courses.All(c => c.IsValid(false));

            if(result)
            {
                var queryUri = GetUri($"/courses");

                var coursesJson = JsonConvert.SerializeObject(courses, _serializerSettings);

                var coursesStringContent = new StringContent(coursesJson, Encoding.UTF8, "application/json" );
                var response = await _httpClient.PostAsync(queryUri, coursesStringContent);

                result = response.IsSuccessStatusCode;
            }

            return result;
        }

        public PaginatedList<Course> GetCourses(QueryFilter filter)
        {
            var queryUri = GetUri("/courses", filter);

            return GetObjects<PaginatedList<Course>>(queryUri);
        }

        public TotalCountResult GetCoursesTotalCount(QueryFilter filter)
        {
            var queryUri = GetUri("/courses/total", filter);

            return GetObjects<TotalCountResult>(queryUri);
        }

        public List<Subject> GetSubjects()
        {
            var queryUri = GetUri("/subjects");

            return GetObjects<List<Subject>>(queryUri);
        }

        public List<SubjectArea> GetSubjectAreas()
        {
            var queryUri = GetUri("/subjectareas");

            return GetObjects<List<SubjectArea>>(queryUri);
        }

        public List<FeeCaps> GetFeeCaps()
        {
            var queryUri = GetUri("/feecaps");

            return GetObjects<List<FeeCaps>>(queryUri);
        }

        public List<Provider> GetProviderSuggestions(string query)
        {
            var buider = new UriBuilder(new Uri(_apiUri));
            buider.Path += "/providers/suggest";
            buider.Query = "query=" + HttpUtility.UrlEncode(query);

            return GetObjects<List<Provider>>(buider.Uri) ?? new List<Provider>();
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

        private Uri GetUri(string apiPath, QueryFilter filter = null)
        {
            var uri = new Uri(_apiUri);
            var builder = new UriBuilder(uri);
            if (!builder.Path.EndsWith('/') && !apiPath.StartsWith('/')) { builder.Path += '/'; }
            else if (builder.Path.EndsWith('/') && apiPath.StartsWith('/')) { apiPath = apiPath.Substring(1); }
            builder.Path += apiPath;
            if (filter != null) { builder.Query = filter.AsQueryString(); }
            return builder.Uri;
        }
    }
}
