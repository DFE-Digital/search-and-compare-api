#!/usr/bin/env bash

deployZip=deploy.zip

echo "Removing old $deployZip if any..."
[ -e $deployZip ] && rm $deployZip

echo "running dotnet publish..."
dotnet publish src/geocoder --configuration Release

echo "creating zip..."
zip $deployZip -r src/geocoder/bin/Release/netcoreapp2.0/publish

echo "uploading to azure WebJob..."
curl -X PUT -u "$1" --data-binary @$deployZip --header "Content-Type: application/zip" --header "Content-Disposition: attachment; filename=$deployZip" https://$2.scm.azurewebsites.net/api/triggeredwebjobs/geocoder/

echo "Deploy complete."
