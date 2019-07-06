#!/bin/bash -v
set -e
source metadata.sh
cd TestProject
nuget add ../MiniDataBase.$version.nupkg -Source ./packages
dotnet add TestProject/TestProject.csproj package MiniDatabase -s ./packages
