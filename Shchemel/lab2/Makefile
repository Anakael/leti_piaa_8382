all:
	dotnet build --configuration Release Source
	dotnet publish --configuration Release Source
	cp Source/bin/Release/*/publish/* .
clean:
	rm -rf Source/bin Source/obj lab2
