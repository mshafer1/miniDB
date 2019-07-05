#!/bin/bash
if [ "$TRAVIS_BRANCH" == "release" ]; then
nuget push $id.$version.nupkg -source nuget.org -apiKey $NUGET_API_KEY
fi
