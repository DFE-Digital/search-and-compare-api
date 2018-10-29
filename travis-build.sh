#!/usr/bin/env bash
set -e
dotnet restore ./src/api/SearchAndCompareApi.csproj -v q
dotnet restore ./src/domain/SearchAndCompareDomain.csproj -v q
dotnet restore ./tests/SearchAndCompareApi.Tests.csproj -v q

dotnet test ./tests/SearchAndCompareApi.Tests.csproj
dotnet test ./tests/SearchAndCompareApi.Tests.csproj --filter TestCategory="Integration"