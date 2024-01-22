#!/usr/bin/pwsh
Clear-Host

if ($PSVersionTable.PSVersion.Major -lt 6)
{
    Write-Host 'Restarting using pwsh...'
    pwsh $PSCommandPath
    return
}

Write-Host 'Clearing bin/obj...' -ForegroundColor Cyan
Remove-Item -LiteralPath UI.WinForms.Msil/bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath UI.WinForms.Msil/obj -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath UI.Avalonia/bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath UI.Avalonia/obj -Recurse -Force -ErrorAction SilentlyContinue

if (($PSVersionTable.Platform -eq 'Win32NT') -or $IsWindows)
{
    Write-Host 'Building Windows binaries...' -ForegroundColor Cyan
    dotnet build -v:q -r win-x64 -f net8.0-windows --self-contained -c Release UI.Avalonia/UI.Avalonia.csproj
    dotnet publish -v:q -r win-x64 -f net8.0-windows --self-contained -c Release -o distrib/gui/win/ UI.Avalonia/UI.Avalonia.csproj /p:PublishTrimmed=False /p:PublishSingleFile=True
}

Write-Host 'Building Linux binary...' -ForegroundColor Cyan
dotnet build -v:q -r linux-x64 -f net8.0 --self-contained -c Release UI.Avalonia/UI.Avalonia.csproj
dotnet publish -v:q -r linux-x64 -f net8.0 --self-contained -c Release -o distrib/gui/lin/ UI.Avalonia/UI.Avalonia.csproj /p:PublishTrimmed=False /p:PublishSingleFile=True
if (($LASTEXITCODE -eq 0) -and (($PSVersionTable.Platform -eq 'Unix') -or $IsLinux))
{
    chmod +x distrib/gui/lin/ps3-disc-dumper
}

Write-Host 'Clearing extra files in distrib...' -ForegroundColor Cyan
Get-ChildItem -LiteralPath distrib -Include *.pdb,*.config -Recurse | Remove-Item

Write-Host 'Zipping...' -ForegroundColor Cyan
if (Test-Path -LiteralPath distrib/gui/win-legacy/ps3-disc-dumper.exe)
{
    Compress-Archive -LiteralPath distrib/gui/win-legacy/ps3-disc-dumper.exe -DestinationPath distrib/ps3-disc-dumper_windows_legacy_NEW.zip -CompressionLevel Optimal -Force
}
if (Test-Path -LiteralPath distrib/gui/win/ps3-disc-dumper.exe)
{
    Compress-Archive -LiteralPath distrib/gui/win/ps3-disc-dumper.exe -DestinationPath distrib/ps3-disc-dumper_windows_NEW.zip -CompressionLevel Optimal -Force
}
if (Test-Path -LiteralPath distrib/cli/lin/ps3-disc-dumper)
{
    Compress-Archive -LiteralPath distrib/cli/lin/ps3-disc-dumper -DestinationPath distrib/ps3-disc-dumper_linux_legacy_NEW.zip -CompressionLevel Optimal -Force
}
if (Test-Path -LiteralPath distrib/gui/lin/ps3-disc-dumper)
{
    Compress-Archive -LiteralPath distrib/gui/lin/ps3-disc-dumper -DestinationPath distrib/ps3-disc-dumper_linux_NEW.zip -CompressionLevel Optimal -Force
}

Write-Host 'Done' -ForegroundColor Cyan


