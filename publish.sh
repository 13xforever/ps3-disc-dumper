#!/bin/sh
# sudo apt-get install libcurl4-openssl-dev libkrb5-dev zlib1g-dev curl
rm -rf UI.Console/bin
rm -rf UI.Console/obj
dotnet publish -r linux-x64 -c Release -o distrib/cli/lin/ UI.Console/UI.Console.csproj /p:PublishTrimmed=true;PublishSingleFile=true
#env CppCompilerAndLinker="clang-6.0" dotnet publish -r linux-x64 -c Release-Core /p:DefineConstants="NATIVE"
