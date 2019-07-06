#!/bin/bash -v
set -e
source metadata.sh

FEED_PATH=`pwd`/_feed

nuget init .\_packages $FEED_PATH
cd TestProject

cp -f _testProject.csproj TestProject.csproj
sed -i 's;<version>;'"$version"';' TestProject.csproj

cp -f _packages.config packages.config
sed -i 's;<version>;'"$version"';' packages.config

nuget restore TestProject.csproj -Source $FEED_PATH

cd ..

xbuild /p:Configuration=Release TestProject.sln
