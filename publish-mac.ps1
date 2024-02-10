#!/usr/bin/pwsh
Clear-Host

if ($PSVersionTable.PSVersion.Major -lt 6)
{
    Write-Host 'Restarting using pwsh...'
    pwsh $PSCommandPath
    return
}

Write-Host 'Clearing bin/obj...' -ForegroundColor Cyan
Remove-Item -LiteralPath UI.Avalonia/bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath UI.Avalonia/obj -Recurse -Force -ErrorAction SilentlyContinue

if (-not $IsMacOS)
{
    Write-Error "Building macOS binary requires macOS"
    return
}

Write-Host 'Building macOS binary...' -ForegroundColor Cyan
dotnet publish -v:q -r osx-arm64 -f net8.0-macos --self-contained -c MacOS -o distrib/gui/mac/ UI.Avalonia/UI.Avalonia.csproj /p:PublishTrimmed=False /p:PublishSingleFile=True
if (($LASTEXITCODE -eq 0) -and ($IsMacOS -or ($PSVersionTable.Platform -eq 'Unix')))
{
    chmod +x distrib/gui/mac/ps3-disc-dumper
}

Write-Host 'Clearing extra files in distrib...' -ForegroundColor Cyan
Get-ChildItem -LiteralPath distrib -Include *.pdb,*.config -Recurse | Remove-Item

Write-Host 'Zipping...' -ForegroundColor Cyan
if (Test-Path -LiteralPath distrib/gui/mac/ps3-disc-dumper)
{
    Compress-Archive -Path 'distrib/gui/mac/*' -DestinationPath distrib/ps3-disc-dumper_macos_NEW.zip -CompressionLevel Optimal -Force
}

Write-Host 'Done' -ForegroundColor Cyan


