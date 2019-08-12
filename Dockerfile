FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build-env
WORKDIR /app

RUN mkdir -p /app/src/api
RUN mkdir -p /app/src/domain

# Copy csproj and restore as distinct layers
COPY ./src/api/SearchAndCompareApi.csproj ./src/api/SearchAndCompareApi.csproj
COPY ./src/domain/SearchAndCompareApiClient.csproj ./src/domain/SearchAndCompareApiClient.csproj
RUN dotnet restore ./src/api/SearchAndCompareApi.csproj
RUN dotnet restore ./src/domain/SearchAndCompareApiClient.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.1 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .
CMD dotnet SearchAndCompareApi.dll
