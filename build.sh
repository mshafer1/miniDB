#/bin/bash -v
set -e

msbuild /p:Configuration=Release MiniDatabase.sln
mono ./testrunner/xunit.runner.console.2.3.1/tools/net452/xunit.console.exe ./DbXunitTests/bin/Release/DbXunitTests.dll ./MutexLockTests/bin/Release/MutexLockTests.dll -parallel none -noshadow

source ./setup_nuspec.sh

nuget pack ./MiniDB.nuspec -OutputDirectory ./_packages

source ./testProject.sh
