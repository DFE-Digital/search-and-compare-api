# Search and Compare API project

## Building API domain nuget package

First, bump the version number in SearchAndCompareDomain.csproj. The -beta suffix is appended automatically.

```
cd src/domain
dotnet pack -o publish -c Release --version-suffix beta
dotnet nuget push publish/DFE.SearchAndCompare.Domain.0.1.3-beta.nupkg -s https://www.nuget.org -k <NUGET_API_KEY>
```

Make sure to change the package to reflect your version number, e.g. change "0.1.3". You'll also need to put in the API key for nuget.