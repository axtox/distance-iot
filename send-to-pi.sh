dotnet publish -r linux-arm
pushd ./bin/Debug/net6.0/linux-arm/publish
scp -r . pi:/home/hud/Projects/Distance
popd