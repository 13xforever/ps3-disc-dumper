#!/bin/sh
# sudo apt-get install libcurl4-openssl-dev libkrb5-dev zlib1g-dev curl
env CppCompilerAndLinker="clang-6.0" dotnet publish -r linux-x64 -c Release-Core /p:DefineConstants="NATIVE"