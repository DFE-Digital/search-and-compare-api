using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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

        public Fees GetLatestFees()
        {
            var queryUri = GetUri("/fees", null);

            return GetObjects<Fees>(queryUri);
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