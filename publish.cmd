@echo off
rmdir /S /Q "UI.WinForms.Msil/bin"
rmdir /S /Q "UI.WinForms.Msil/obj"
dotnet publish -r win-x64 -c Release /p:PublishTrimmed=true;PublishSingleFile=true -o distrib/gui/win/ UI.WinForms.Msil/UI.WinForms.Msil.csproj
::dotnet publish -r win-x64 -c Release-Core /p:Platform="Any CPU" /p:DefineConstants="NATIVE"