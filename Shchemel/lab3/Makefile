all:
	dotnet build --configuration Release src/lab3
	dotnet publish --configuration Release src/lab3
	cp src/lab3/bin/Release/*/publish/* .
clean::wq
	rm -rf src/lab3/bin src/lab3/obj lab*
