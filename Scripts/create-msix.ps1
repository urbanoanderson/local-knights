<#
.SYNOPSIS
    Builds an unsigned MSIX package for submission to Microsoft Store.

.DESCRIPTION
    Publishes the Windows desktop app as a self-contained win-x64 app, creates the
    package manifest and required logo assets, and packs them with MakeAppx.exe.
    Microsoft Store signs the package after submission.

.EXAMPLE
    .\create-msix.ps1

.EXAMPLE
    .\create-msix.ps1 -PackageIdentityName "urbanoanderson.LocalKnights" `
        -PackagePublisher "CN=266293A1-A9B9-44EC-86B1-867682380E7D"
#>
param(
    [string]$ProjectPath = (Join-Path $PSScriptRoot "..\LocalKnights.WindowsDesktop\LocalKnights.WindowsDesktop.csproj"),
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$PackageIdentityName = "urbanoanderson.LocalKnights",
    [string]$PackagePublisher = "CN=266293A1-A9B9-44EC-86B1-867682380E7D",
    [string]$PackageDisplayName = "Local Knights",
    [string]$PublisherDisplayName = "urbanoanderson",
    [string]$PackageDescription = "Local Knights chess club manager",
    [string]$OutputDir = (Join-Path $PSScriptRoot "..\LocalKnights.WindowsDesktop\bin\Packages"),
    [string]$PackageName = "",
    [string]$MakeAppxPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Find-MakeAppx {
    param([string]$ProvidedPath)

    if ($ProvidedPath) {
        if (-not (Test-Path -LiteralPath $ProvidedPath -PathType Leaf)) {
            throw "MakeAppx.exe was not found at '$ProvidedPath'."
        }
        return (Resolve-Path -LiteralPath $ProvidedPath).Path
    }

    $onPath = Get-Command "makeappx.exe" -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    $sdkBin = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"
    if (Test-Path -LiteralPath $sdkBin) {
        $candidates = Get-ChildItem -Path $sdkBin -Directory |
            Where-Object { $_.Name -match '^\d+(\.\d+)+$' } |
            Sort-Object { [version]$_.Name } -Descending |
            ForEach-Object { Join-Path $_.FullName "x64\makeappx.exe" } |
            Where-Object { Test-Path -LiteralPath $_ }
        if ($candidates) { return ($candidates | Select-Object -First 1) }
    }

    throw "MakeAppx.exe was not found. Install the Windows SDK or pass -MakeAppxPath."
}

function ConvertTo-ManifestText {
    param([string]$Value)
    return [System.Security.SecurityElement]::Escape($Value)
}

$projectFullPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$projectXml = [xml](Get-Content -LiteralPath $projectFullPath -Raw)
$versionNode = $projectXml.SelectSingleNode("/Project/PropertyGroup/Version")
if (-not $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
    throw "Could not find a project <Version> value in '$projectFullPath'."
}

$versionParts = @($versionNode.InnerText.Split("."))
if ($versionParts.Count -lt 2 -or $versionParts.Count -gt 4) {
    throw "Version '$($versionNode.InnerText)' must contain two to four numeric components."
}
foreach ($part in $versionParts) {
    if ($part -notmatch '^\d+$' -or [int]$part -gt 65535) {
        throw "Version '$($versionNode.InnerText)' has an invalid component: '$part'."
    }
}
while ($versionParts.Count -lt 4) { $versionParts += "0" }
$packageVersion = $versionParts -join "."

if ([string]::IsNullOrWhiteSpace($PackageIdentityName) -or
    [string]::IsNullOrWhiteSpace($PackagePublisher)) {
    throw "Package identity name and publisher must match Partner Center > Product identity."
}
if ($Runtime -ne "win-x64") {
    throw "This project currently packages only win-x64. Unsupported runtime: '$Runtime'."
}

$makeappx = Find-MakeAppx -ProvidedPath $MakeAppxPath
$projectDir = Split-Path -Parent $projectFullPath
$iconPath = Join-Path $projectDir "Resources\Icon\logo-bg.png"
if (-not (Test-Path -LiteralPath $iconPath -PathType Leaf)) {
    throw "Required package logo was not found: '$iconPath'."
}

$publishDir = Join-Path $env:TEMP ("LocalKnightsPublish_{0}" -f [Guid]::NewGuid().ToString("N"))
if ([string]::IsNullOrWhiteSpace($PackageName)) {
    $PackageName = "LocalKnights_$($packageVersion)_x64.msix"
}
if ([System.IO.Path]::GetFileName($PackageName) -ne $PackageName) {
    throw "-PackageName must be a filename, not a path."
}
$outputFullPath = [System.IO.Path]::GetFullPath($OutputDir)
$packagePath = Join-Path $outputFullPath $PackageName

try {
    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

    Write-Host "Publishing $Runtime app..."
    & dotnet publish $projectFullPath -c $Configuration -r $Runtime `
        /p:PublishSingleFile=true /p:SelfContained=true `
        /p:EnableCompressionInSingleFile=true /p:DebugSymbols=false `
        /p:DebugType=None -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE."
    }
    Get-ChildItem -Path $publishDir -Filter "*.pdb" -File |
        Remove-Item -Force

    $appExe = Get-ChildItem -Path $publishDir -Filter "*.exe" |
        Where-Object { $_.Name -notlike "*vshost*" } |
        Select-Object -First 1
    if (-not $appExe) { throw "No application executable was produced by dotnet publish." }

    $assetsDir = Join-Path $publishDir "Assets"
    New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null
    Add-Type -AssemblyName System.Drawing

    $sourceImage = [System.Drawing.Image]::FromFile($iconPath)
    try {
        foreach ($asset in @(@{ Name = "Square44x44Logo.png"; Size = 44 }, @{ Name = "Square150x150Logo.png"; Size = 150 })) {
            $bitmap = New-Object System.Drawing.Bitmap($asset.Size, $asset.Size)
            try {
                $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
                try {
                    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                    $graphics.DrawImage($sourceImage, 0, 0, $asset.Size, $asset.Size)
                    $bitmap.Save((Join-Path $assetsDir $asset.Name), [System.Drawing.Imaging.ImageFormat]::Png)
                }
                finally { $graphics.Dispose() }
            }
            finally { $bitmap.Dispose() }
        }
    }
    finally { $sourceImage.Dispose() }

    $identityNameXml = ConvertTo-ManifestText $PackageIdentityName
    $publisherXml = ConvertTo-ManifestText $PackagePublisher
    $displayNameXml = ConvertTo-ManifestText $PackageDisplayName
    $publisherDisplayNameXml = ConvertTo-ManifestText $PublisherDisplayName
    $descriptionXml = ConvertTo-ManifestText $PackageDescription
    $executableXml = ConvertTo-ManifestText $appExe.Name
    $manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
         IgnorableNamespaces="uap desktop rescap">
  <Identity Name="$identityNameXml" Publisher="$publisherXml" Version="$packageVersion" ProcessorArchitecture="x64" />
  <Properties>
    <DisplayName>$displayNameXml</DisplayName>
    <PublisherDisplayName>$publisherDisplayNameXml</PublisherDisplayName>
    <Description>$descriptionXml</Description>
    <Logo>Assets\Square150x150Logo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.26100.0" />
  </Dependencies>
  <Resources>
    <Resource Language="en-us" />
  </Resources>
  <Applications>
    <Application Id="App" Executable="$executableXml" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="$displayNameXml" Square150x150Logo="Assets\Square150x150Logo.png" Square44x44Logo="Assets\Square44x44Logo.png" Description="$descriptionXml" BackgroundColor="transparent" />
      <Extensions>
        <desktop:Extension Category="windows.fullTrustProcess" EntryPoint="Windows.FullTrustApplication">
          <desktop:FullTrustProcess />
        </desktop:Extension>
      </Extensions>
    </Application>
  </Applications>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
"@

    $manifestPath = Join-Path $publishDir "AppxManifest.xml"
    Set-Content -LiteralPath $manifestPath -Value $manifest -Encoding UTF8
    $null = [xml](Get-Content -LiteralPath $manifestPath -Raw)

    New-Item -ItemType Directory -Path $outputFullPath -Force | Out-Null

    Write-Host "Packing unsigned Store package with MakeAppx..."
    & $makeappx pack /d $publishDir /p $packagePath /o
    if ($LASTEXITCODE -ne 0) {
        throw "MakeAppx failed with exit code $LASTEXITCODE."
    }

    Write-Host "Checking package contents..."
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $manifestEntry = $archive.GetEntry("AppxManifest.xml")
        if (-not $manifestEntry) { throw "The package is missing AppxManifest.xml." }
        $executableEntry = [System.Uri]::EscapeDataString($appExe.Name)
        foreach ($requiredEntry in @(
            "Assets/Square44x44Logo.png",
            "Assets/Square150x150Logo.png",
            "sqlite3mc.dll",
            "chesspairing.dll",
            $executableEntry
        )) {
            if (-not $archive.GetEntry($requiredEntry)) {
                throw "The package is missing required file '$requiredEntry'."
            }
        }

        $reader = New-Object System.IO.StreamReader($manifestEntry.Open())
        try { $packedManifest = [xml]$reader.ReadToEnd() }
        finally { $reader.Dispose() }

        $packedIdentity = $packedManifest.Package.Identity
        if ($packedIdentity.Name -ne $PackageIdentityName -or
            $packedIdentity.Publisher -ne $PackagePublisher -or
            $packedIdentity.Version -ne $packageVersion -or
            $packedIdentity.ProcessorArchitecture -ne "x64") {
            throw "The package manifest identity does not match the requested Store identity and version."
        }
        if ($packedManifest.Package.Properties.PublisherDisplayName -ne $PublisherDisplayName) {
            throw "The package manifest publisher display name does not match '$PublisherDisplayName'."
        }
        if ($archive.GetEntry("AppxSignature.p7x")) {
            throw "Store submission package must be unsigned; an AppxSignature.p7x was found."
        }
    }
    finally { $archive.Dispose() }

    Write-Host "Store upload package created: $packagePath"
    Write-Host "Identity: $PackageIdentityName"
    Write-Host "Publisher: $PackagePublisher"
    Write-Host "Version: $packageVersion"
    Write-Host "The Microsoft Store signs the package after submission."
}
finally {
    if (Test-Path -LiteralPath $publishDir) {
        Remove-Item -LiteralPath $publishDir -Recurse -Force
    }
}
