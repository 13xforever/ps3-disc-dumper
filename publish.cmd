@echo off
rmdir /S /Q "UI.WinForms.Msil/bin"
rmdir /S /Q "UI.WinForms.Msil/obj"
dotnet build -c Release -r win-x64 UI.WinForms.Msil/UI.WinForms.Msil.csproj
dotnet publish -r win-x64 -c Release -o distrib/gui/win/ UI.WinForms.Msil/UI.WinForms.Msil.csproj /p:PublishTrimmed=true;PublishSingleFile=true
dotnet build -c Release -r linux-x64 UI.Console/UI.Console.csproj
dotnet publish -r linux-x64 -c Release -o distrib/cli/lin/ UI.Console/UI.Console.csproj /p:PublishTrimmed=true;PublishSingleFile=true
