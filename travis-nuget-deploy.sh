ApiKey=$1
Source=$2

echo $Source
echo "Source"
dotnet pack ./src/domain
dotnet nuget push ./src/Domain/bin/**/*.nupkg -k $ApiKey -s https://www.nuget.org || echo "Nuget deploy skipped"