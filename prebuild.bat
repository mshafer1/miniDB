pushd %~dp0\DbXunitTests
if not exist GeneratedCode mkdir GeneratedCode

pushd %~dp0\MiniDB\bin\Debug

%~dp0/packages/HavenSoft.AutoImplement.1.1.1/AutoImplement ./MiniDB.dll IUndoRedoManager

move /Y StubUndoRedoManager.cs  %~dp0/DbXunitTests/GeneratedCode