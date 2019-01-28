#!/bin/bash
set -e

# script for deploying to any environment, choosing the matching deploy password environment variable

# arg 1: environment to deploy to
env=$1

# convert env to uppercase to match environment variable case
upperenv=`echo "$env" | tr '[:lower:]' '[:upper:]'`

eval "password=\$APP_CREDENTIALS_${upperenv}_PWD"

git fetch --unshallow
git push --force https://\$bat-$env-search-and-compare-api-app:$password@bat-$env-search-and-compare-api-app.scm.azurewebsites.net:443/bat-$env-search-and-compare-api-app.git HEAD:refs/heads/master
