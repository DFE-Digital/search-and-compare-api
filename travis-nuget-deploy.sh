ApiKey=$1
Source=$2

dotnet pack ./src/domain
dotnet nuget push ./src/domain/bin/**/*.nupkg -k $ApiKey -s $Source || echo "Nuget deploy skipped"