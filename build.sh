#/bin/bash -v
set -e

# clear out build artifacts
echo "deleting build artifacts"
rm -rf */bin/
rm -rf */obj/


msbuild /p:Configuration=Release MiniDatabase.sln
mono ./testrunner/xunit.runner.console.2.3.1/tools/net452/xunit.console.exe ./DbXunitTests/bin/Release/DbXunitTests.dll -parallel none -noshadow
mono ./testrunner/xunit.runner.console.2.3.1/tools/net452/xunit.console.exe ./MutexLockTests/bin/Release/MutexLockTests.dll -parallel none -noshadow

source ./setup_nuspec.sh

nuget pack ./MiniDB.nuspec -OutputDirectory ./_packages

source ./testProject.sh
