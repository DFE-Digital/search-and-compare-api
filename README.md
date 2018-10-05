# Search and Compare API project

[<img src="https://img.shields.io/nuget/v/DFE.SearchAndCompare.Domain.svg">](https://www.nuget.org/packages/DFE.SearchAndCompare.Domain)
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

## Building API domain nuget package

First, bump the version number in SearchAndCompareDomain.csproj. The -beta suffix is appended automatically.

```
cd src/domain
dotnet pack -o publish -c Release --version-suffix beta
dotnet nuget push publish/DFE.SearchAndCompare.Domain.0.10.0-beta.nupkg -s https://www.nuget.org -k <NUGET_API_KEY>
```

Make sure to change the package to reflect your version number, e.g. change "0.1.3". You'll also need to put in the API key for nuget.

## Using the domain nuget package locally

1. Run `pack` (See above)
2. Reference the package in the publish folder by adding a local package source in `nuget.config`.
   [Example in UI project](https://github.com/DFE-Digital/search-and-compare-ui/blob/dd22365f4ae476c9a0126d6acbd60020a6a10858/Nuget.config).

    <configuration><packageSources><add key="local-packages" value="../search-and-compare-api/src/domain/publish" />

3.  Run `dotnet nuget locals all --clear` to remove the stale local cache (to avoid needing to bump the version number during local development
    of API + UI
4. Build the referencing project (presumably the UI).

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
