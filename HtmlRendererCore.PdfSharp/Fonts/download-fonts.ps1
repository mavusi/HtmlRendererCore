# PowerShell script to download Liberation Fonts
# This script downloads Liberation Fonts from GitHub and extracts them to the Fonts directory

$ErrorActionPreference = "Stop"

# Configuration
$FontsDir = $PSScriptRoot
$Version = "2.1.5"
$DownloadUrl = "https://github.com/liberationfonts/liberation-fonts/files/7261482/liberation-fonts-ttf-$Version.tar.gz"
$TempDir = Join-Path $env:TEMP "liberation-fonts-temp"
$TarFile = Join-Path $TempDir "liberation-fonts.tar.gz"

Write-Host "Downloading Liberation Fonts v$Version..." -ForegroundColor Green

# Create temp directory
if (Test-Path $TempDir) {
    Remove-Item $TempDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TempDir | Out-Null

try {
    # Download the font archive
    Write-Host "Downloading from $DownloadUrl..."
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $TarFile -UseBasicParsing

    Write-Host "Extracting fonts..." -ForegroundColor Green

    # Extract tar.gz (requires tar command available in Windows 10+)
    tar -xzf $TarFile -C $TempDir

    # Find the extracted directory
    $ExtractedDir = Get-ChildItem -Path $TempDir -Directory | Select-Object -First 1

    if ($null -eq $ExtractedDir) {
        throw "Could not find extracted directory"
    }

    # Copy TTF files to Fonts directory
    $TtfFiles = Get-ChildItem -Path $ExtractedDir.FullName -Filter "*.ttf" -Recurse

    if ($TtfFiles.Count -eq 0) {
        throw "No TTF files found in the archive"
    }

    Write-Host "Copying font files to $FontsDir..." -ForegroundColor Green

    foreach ($file in $TtfFiles) {
        $destPath = Join-Path $FontsDir $file.Name
        Copy-Item $file.FullName -Destination $destPath -Force
        Write-Host "  Copied: $($file.Name)" -ForegroundColor Gray
    }

    Write-Host ""
    Write-Host "Successfully downloaded and installed $($TtfFiles.Count) font files!" -ForegroundColor Green
    Write-Host "Liberation Fonts are licensed under SIL Open Font License 1.1" -ForegroundColor Yellow
}
catch {
    Write-Error "Failed to download fonts: $_"
    Write-Host ""
    Write-Host "Manual download instructions:" -ForegroundColor Yellow
    Write-Host "1. Visit: https://github.com/liberationfonts/liberation-fonts/releases" -ForegroundColor Gray
    Write-Host "2. Download liberation-fonts-ttf-$Version.tar.gz" -ForegroundColor Gray
    Write-Host "3. Extract the TTF files to: $FontsDir" -ForegroundColor Gray
    exit 1
}
finally {
    # Cleanup
    if (Test-Path $TempDir) {
        Remove-Item $TempDir -Recurse -Force
    }
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Build the project to embed the fonts" -ForegroundColor Gray
Write-Host "2. The fonts will be available in Docker and restricted environments" -ForegroundColor Gray
