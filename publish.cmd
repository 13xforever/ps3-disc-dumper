@echo off
rmdir /S /Q "UI.WinForms.Msil/bin"
rmdir /S /Q "UI.WinForms.Msil/obj"
dotnet build -c Release -r win-x64 --self-contained true UI.WinForms.Msil/UI.WinForms.Msil.csproj
dotnet publish -r win-x64 --self-contained true -c Release -o distrib/gui/win/ UI.WinForms.Msil/UI.WinForms.Msil.csproj /p:PublishTrimmed=false;PublishSingleFile=true
dotnet build -c Release -r linux-x64 --self-contained true UI.Console/UI.Console.csproj
dotnet publish -r linux-x64 --self-contained true -c Release -o distrib/cli/lin/ UI.Console/UI.Console.csproj /p:PublishTrimmed=true;PublishSingleFile=true
