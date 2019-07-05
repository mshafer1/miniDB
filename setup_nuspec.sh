source metadata.sh
cp MiniDB/MiniDB.nuspec ./
sed -i 's;\$version\$;'"$version"';' MiniDB.nuspec
sed -i 's;\$title\$;'"$title"';' MiniDB.nuspec
sed -i 's;\$id\$;'"$id"';' MiniDB.nuspec
sed -i 's;\$authors\$;'"$authors"';' MiniDB.nuspec
sed -i 's;\$owners\$;'"$owners"';' MiniDB.nuspec
sed -i 's;\$description\$;'"$description"';' MiniDB.nuspec
