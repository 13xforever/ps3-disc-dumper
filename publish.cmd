@echo off
dotnet publish -r win-x64 -c Release-Core /p:Platform="Any CPU" /p:DefineConstants="NATIVE"