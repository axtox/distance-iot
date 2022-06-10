dotnet publish -r linux-arm -c Debug 
pushd ./bin/Debug/net6.0/linux-arm/publish
sshpass -p 6464 scp -r . pi:/home/hud/Projects/Distance
popd