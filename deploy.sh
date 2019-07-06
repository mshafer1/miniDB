#!/bin/bash
if [ "$TRAVIS_BRANCH" == "release" ]; then
nuget push _packages/$id.$version.nupkg -source nuget.org -apiKey $NUGET_API_KEY
fi
