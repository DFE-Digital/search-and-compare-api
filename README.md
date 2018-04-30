# Search and Compare API project

## About

This repo provides a dotnet core solution containing:

* An API + postgres database for managing course data and interacting with the UCAS website.
* A client library to provide easy usage of the above API, including a shared set of domain classes.
* Regression tests.

## Coding

It can be worked on in either VSCode or Visual Studio 2017 as preferred.

The domain project follows https://semver.org/ version numbering.

## Running the api locally

In a windows command prompt:

    cd src\api
    set ASPNETCORE_URLS=http://*:5001 && dotnet run

## Building API domain nuget package

First, bump the version number in SearchAndCompareDomain.csproj. The -beta suffix is appended automatically.

```
cd src/domain
dotnet pack -o publish -c Release --version-suffix beta
dotnet nuget push publish/DFE.SearchAndCompare.Domain.0.1.3-beta.nupkg -s https://www.nuget.org -k <NUGET_API_KEY>
```

Make sure to change the package to reflect your version number, e.g. change "0.1.3". You'll also need to put in the API key for nuget.

## Using the domain nuget package locally

1. Run `pack` (see above)
2. Reference the package in the publish folder by adding a local package source in `nuget.config`

    <configuration><packageSources><add key="local-packages" value="../search-and-compare-api/src/domain/publish" />

[example in ui project](https://github.com/DFE-Digital/search-and-compare-ui/blob/dd22365f4ae476c9a0126d6acbd60020a6a10858/Nuget.config)
