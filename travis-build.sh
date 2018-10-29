#!/usr/bin/env bash
set -e
dotnet restore ./src/api/SearchAndCompareApi.csproj -v m
dotnet restore ./src/domain/SearchAndCompareDomain.csproj -v m
dotnet restore ./tests/SearchAndCompareApi.Tests.csproj -v m

dotnet test ./tests/SearchAndCompareApi.Tests.csproj
dotnet test ./tests/SearchAndCompareApi.Tests.csproj --filter TestCategory="Integration"