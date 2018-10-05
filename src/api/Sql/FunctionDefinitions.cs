namespace GovUk.Education.SearchAndCompare.Api.Sql
{
    /// <summary>
    /// A quick hack to make the migrations of replacable sql more source-control friendly.
    /// If you link your migration code back to these strings you'll be able to see diffs in git as they change
    /// which you can't with EF migration files.
    /// </summary>
    public static class FunctionDefinitions
    {
        public const string CourseDistance = @"
CREATE OR REPLACE
FUNCTION course_distance( lat DOUBLE PRECISION, lon DOUBLE PRECISION, rad DOUBLE PRECISION) 
RETURNS TABLE (""CourseId"" integer, ""CampusId"" integer, ""Distance"" double precision)
AS $$
	-- Returns course with a provider or campus within the specified radius of the specificied latitude/longitude location.
	-- If the match was a provider then CampusId is null, if it was a campus then CampusId will be the nearest campus for that course.
	-- CourseIds will be unique in the returned table
	-- Usage: select * FROM location_distance(51.5073509,-0.1277583,8046.7)
	SELECT distinct on (""CourseId"") -- only take nearest entry for each course https://www.postgresql.org/docs/current/static/sql-select.html#SQL-DISTINCT
		""CourseId"", ""CampusId"", ""Distance""
	FROM
	(
		(
			-- find by provider location
			SELECT course.""Id"" ""CourseId"", earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) AS ""Distance"", null as ""CampusId""
			FROM ""course""
				JOIN location loc ON course.""ProviderLocationId"" = loc.""Id""
			WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"")
				AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) <= rad
		)
		-- A UNION is used rather than a JOIN as it was found to be an order of magnitude faster.
		UNION ALL
		(
			-- find by nearest campus location for each course
			SELECT course.""Id"" ""CourseId"", earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) AS ""Distance"", campus.""Id"" as ""CampusId""
			FROM ""course""
				JOIN campus on campus.""CourseId"" = course.""Id""
				JOIN location loc ON campus.""LocationId"" = loc.""Id""            
			WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"")
				AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) <= rad
	  	)
	) as subquery -- alias is required but unused
	order by ""CourseId"", ""Distance"" -- has to be sorted by CourseId first for ""distinct on"" to work
$$ LANGUAGE SQL;
";
    }
}
