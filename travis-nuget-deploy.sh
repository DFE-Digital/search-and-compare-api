ApiKey=$1

dotnet pack ./src/domain
dotnet nuget push ./src/Domain/bin/**/*.nupkg -k $ApiKey -s https://www.nuget.org || echo "Nuget deploy skipped"