ApiKey=$1
ls ./src/domain
ls ./src/Domain/bin/**/*.nupkg
dotnet pack ./src/domain
dotnet nuget push ./src/Domain/bin/**/*.nupkg -k $ApiKey -s https://www.nuget.org || echo "Nuget deploy skipped"