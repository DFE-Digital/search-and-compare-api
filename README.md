# Search and Compare API project

[<img src="https://img.shields.io/nuget/v/GovUk.Education.SearchAndCompare.ApiClient.svg">](https://www.nuget.org/packages/GovUk.Education.SearchAndCompare.ApiClient)
[<img src="https://api.travis-ci.org/DFE-Digital/search-and-compare-api.svg?branch=master">](https://travis-ci.org/DFE-Digital/search-and-compare-api?branch=master)

## About

This repo provides a dotnet core solution containing:

* An API + postgres database for searching course data
* A client library to provide easy usage of the above API, including a shared set of domain classes.
* Regression tests.

The main client for this API and library is https://github.com/DFE-Digital/search-and-compare-ui

## Coding

It can be worked on in either VSCode or Visual Studio 2017 as preferred.

The domain project follows https://semver.org/ version numbering.

## Running the API locally

In a windows command prompt:

    cd src\api
    set ASPNETCORE_URLS=http://*:5001 && dotnet run

## Logging

Logging is configured in `appsettings.json`, and values in there can be overridden with environment variables.

Powershell:

    $env:Serilog:MinimumLevel="Debug"
    dotnet run

Command prompt

    set Serilog:MinimumLevel=Debug
    dotnet run

For more information see:

* https://github.com/serilog/serilog-settings-configuration
* https://nblumhardt.com/2016/07/serilog-2-minimumlevel-override/

Serilog has been configured to spit logs out to both the console
(for `dotnet run` testing & development locally) and Application Insights.

Set the `APPINSIGHTS_INSTRUMENTATIONKEY` environment variable to tell Serilog the application insights key.

## Error tracking

This app sends exceptions and errors into [Sentry](https://sentry.io). To enable the integration,
set the `SENTRY_DSN` environment variable.

## Importing/Publishing courses

### Add the api key

```bash
# from .\src\api>
dotnet user-secrets set api:key the-api-key
```
### Client consumption

```csharp
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "the-api-key");

var api = SearchAndCompareApi(httpClient, "http://localhost:5001");

var courses = new List<Course>()
{
    // course1
};
bool result = api.SaveCoursesAsync(courses);
```

## Migrations

### List

    cd src\api
    dotnet ef migrations list

### Add

    cd src\api
    dotnet ef migrations add [MigrationName]

## Shutting down the service and showing the off line page.
Rename the file "app_offline.htm.example" in the root folder to "app_offline.htm"

