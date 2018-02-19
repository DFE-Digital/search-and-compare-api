using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using GovUk.Education.SearchAndCompare.Domain.Filters.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Domain.Filters
{
    public class QueryFilter
    { 
        public int? page { get; set; }

        public double? lng { get; set; }

        public double? lat { get; set; }

        public int? rad { get; set; }

        public string loc { get; set; }

        public string subjects { get; set; }

        public int? sortby { get; set; }

        [IgnoreDataMemberAttribute]
        public List<int> SelectedSubjects { 
            get {            
                List<int> subjectFilterIds = new List<int> ();
                if (!string.IsNullOrEmpty(subjects))
                {
                    subjectFilterIds = subjects.Split(',')?.Select(int.Parse).ToList();
                }
                return subjectFilterIds;
            }

            set {
                subjects = string.Join(",", value);
            }
        }
        
        [IgnoreDataMemberAttribute]
        public Coordinates Coordinates
        {
            get {
                return lng.HasValue && lat.HasValue && rad.HasValue 
                    ? new Coordinates(lat.Value, lng.Value, null, loc) 
                    : null;
            }
        }

        [IgnoreDataMemberAttribute]
        public RadiusOption? RadiusOption
        {
            get {
                return rad.HasValue ? (RadiusOption?) rad.Value : null;
            }
        }

        [IgnoreDataMemberAttribute]
        public SortByOption? SortBy
        {
            get {
                return sortby.HasValue ? (SortByOption?) sortby.Value : null;
            }
        }

        [IgnoreDataMemberAttribute]
        public IEnumerable<SortByOption> ValidSortings
        {
            get {
                return Enum.GetValues(typeof(SortByOption)).Cast<SortByOption>()
                           .Where(x => x != SortByOption.Distance || RadiusOption != null);
            }
        }

        public string AsQueryString()
        {
            var properties = from property in this.GetType().GetProperties()
                where property.GetCustomAttribute(typeof(IgnoreDataMemberAttribute)) == null && property.GetValue(this, null) != null
                select property.Name + "=" + HttpUtility.UrlEncode(property.GetValue(this, null).ToString());

            return String.Join("&", properties.ToArray());
        }
    }
}