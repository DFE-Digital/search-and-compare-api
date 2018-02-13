#!/usr/bin/env bash
set -e
dotnet restore ./src/api/SearchAndCompareApi.csproj
dotnet restore ./src/domain/SearchAndCompareDomain.csproj
dotnet restore ./tests/SearchAndCompareApi.Tests.csproj

dotnet test ./tests/SearchAndCompareApi.Tests.csproj
dotnet test ./tests/SearchAndCompareApi.Tests.csproj --filter TestCategory="Integration"