dotnet publish -r linux-arm -c Release --self-contained true
pushd ./bin/Release/net6.0/linux-arm/publish
sshpass -p 6464 scp -r . pi:/home/hud/Projects/Distance
popd