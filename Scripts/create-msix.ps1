<#
.SYNOPSIS
    Build an MSIX package for the Windows Desktop using MakeAppx + SignTool.

.DESCRIPTION
    Publishes the WPF app as a self-contained single-file build, creates minimal Appx manifest
    and Assets (icons), packs the publish folder into a .msix using MakeAppx.exe and signs
    the package with a provided certificate or a temporary self-signed certificate.

.NOTES
    Requires Windows SDK tools: MakeAppx.exe and SignTool.exe on PATH (installed with Windows SDK).
    If you want a store-ready package, use a real code signing certificate and publish through Partner Center.

USAGE
    .\create-msix.ps1 -PackagePublisher "CN=YourPublisher" -CertificatePath path\to\cert.pfx -CertificatePassword 'pwd'

#>
param(
    [string]$ProjectPath = "${PWD}\LocalKnights.WindowsDesktop\LocalKnights.WindowsDesktop.csproj",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$PackageId = "LocalKnights.Package",
    [string]$PackageName = "LocalKnights.msix",
    [string]$PackagePublisher = "CN=LocalKnightsPublisher",
    [string]$OutputDir = "${PWD}\LocalKnights.WindowsDesktop\bin\Packages",
    [string]$CertificatePath = "",
    [string]$CertificatePassword = "",
    [string]$MakeAppxPath = "",
    [string]$SignToolPath = "",
    [switch]$InstallToLocalMachine
  )

  Set-StrictMode -Version Latest

  # Prefer explicit params, then env vars, then try PATH/common SDK folders
  if (-not $MakeAppxPath -and $env:MAKEAPPX_PATH) { $MakeAppxPath = $env:MAKEAPPX_PATH }
  if (-not $SignToolPath -and $env:SIGNTOOL_PATH) { $SignToolPath = $env:SIGNTOOL_PATH }

  function Find-Tool {
    param(
      [string]$ToolName,
      [string]$ProvidedPath
    )
    if ($ProvidedPath -and (Test-Path $ProvidedPath)) { return (Resolve-Path $ProvidedPath).Path }

    $cmd = Get-Command $ToolName -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $candidates = @()
    $sdkBase1 = "$env:ProgramFiles(x86)\Windows Kits\10\bin"
    $sdkBase2 = "$env:ProgramFiles(x86)\Windows Kits\8.1\bin"
    foreach ($base in @($sdkBase1, $sdkBase2)) {
      if (Test-Path $base) {
        Get-ChildItem -Path $base -Directory -ErrorAction SilentlyContinue | ForEach-Object {
          $possible = Join-Path $_.FullName "x64\$ToolName"
          if (Test-Path $possible) { $candidates += $possible }
          $possible = Join-Path $_.FullName "x86\$ToolName"
          if (Test-Path $possible) { $candidates += $possible }
        }
      }
    }

    if ($candidates.Count -gt 0) { return $candidates[0] }

    return $null
  }

  $makeappxExe = Find-Tool -ToolName "makeappx.exe" -ProvidedPath $MakeAppxPath
  if (-not $makeappxExe) {
    Write-Error "makeappx.exe not found on PATH or in common Windows SDK locations. Install Windows SDK or provide -MakeAppxPath."
    exit 1
  }

  $signtoolExe = Find-Tool -ToolName "signtool.exe" -ProvidedPath $SignToolPath
  if (-not $signtoolExe) {
    Write-Error "signtool.exe not found on PATH or in common Windows SDK locations. Install Windows SDK or provide -SignToolPath."
    exit 1
  }

  if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project file not found at $ProjectPath"
    exit 1
  }

  try {
    [xml]$projectXml = Get-Content -Path $ProjectPath -Raw
    $packageVersion = $projectXml.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -First 1 -ExpandProperty Version
  }
  catch {
    Write-Error "Failed to read project file '$ProjectPath': $_"
    exit 1
  }

  if ([string]::IsNullOrWhiteSpace($packageVersion)) {
    Write-Error "Could not find a <Version> element in '$ProjectPath'."
    exit 1
  }

  $readmePath = Join-Path (Split-Path -Path $ProjectPath -Parent | Split-Path -Parent) "README.md"
  if (Test-Path $readmePath) {
    $versionBadge = "![Version](https://img.shields.io/badge/version-$packageVersion-2ea44f)"
    $readmeContent = Get-Content -Path $readmePath -Raw
    $updatedReadmeContent = [System.Text.RegularExpressions.Regex]::Replace(
      $readmeContent,
      "(?m)^!\[Version\]\(https://img\.shields\.io/badge/version-[^)]+\)$",
      [System.Text.RegularExpressions.MatchEvaluator]{ param($match) $versionBadge },
      1
    )

    if ($updatedReadmeContent -ne $readmeContent) {
      Set-Content -Path $readmePath -Value $updatedReadmeContent -Encoding UTF8
    }
  }

$publishDir = Join-Path -Path $env:TEMP -ChildPath ("LocalKnightsPublish_{0}" -f ([Guid]::NewGuid().ToString()))
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

Write-Host "Publishing project to $publishDir..."
dotnet publish $ProjectPath -c $Configuration -r $Runtime /p:PublishSingleFile=true /p:PublishSelfContained=true /p:EnableCompressionInSingleFile=true -o $publishDir

if ($LASTEXITCODE -ne 0) { Write-Error "dotnet publish failed."; exit 1 }

# Prepare Assets folder and icons
$assetsDir = Join-Path $publishDir "Assets"
New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null

$sourceIcon = Join-Path (Split-Path -Path $ProjectPath -Parent) "Resources\Icon\logo-bg.png"
if (-not (Test-Path $sourceIcon)) { Write-Warning "Source icon not found at $sourceIcon. Assets logos may be missing." }
else {
    Add-Type -AssemblyName System.Drawing
    $sizes = @{"Square44x44Logo.png"=44; "Square150x150Logo.png"=150}
    foreach ($name in $sizes.Keys) {
        try {
            $bmp = [System.Drawing.Image]::FromFile($sourceIcon)
            $thumb = New-Object System.Drawing.Bitmap $sizes[$name], $sizes[$name]
            $g = [System.Drawing.Graphics]::FromImage($thumb)
            $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.DrawImage($bmp, 0, 0, $sizes[$name], $sizes[$name])
            $outPath = Join-Path $assetsDir $name
            $thumb.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
            $g.Dispose(); $thumb.Dispose(); $bmp.Dispose()
        }
        catch {
          Write-Warning "Failed to create asset $($name): $_"
        }
    }
}

# Create minimal AppxManifest.xml
$manifestPath = Join-Path $publishDir "AppxManifest.xml"
$exeName = Get-ChildItem -Path $publishDir -Filter "*.exe" | Where-Object { $_.Name -notlike "*vshost*" } | Select-Object -First 1
if (-not $exeName) { Write-Error "No executable found in publish folder."; exit 1 }

$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" 
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
         IgnorableNamespaces="uap desktop rescap">
  <Identity Name="$($PackageId)" Publisher="$($PackagePublisher)" Version="$($packageVersion)" />
  <Properties>
    <DisplayName>Local Knights</DisplayName>
    <PublisherDisplayName>Local Knights</PublisherDisplayName>
    <Description>Local Knights chess club manager</Description>
    <Logo>Assets\Square150x150Logo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.16299.0" MaxVersionTested="10.0.19041.0" />
  </Dependencies>
  <Resources>
    <Resource Language="en-us" />
  </Resources>
  <Applications>
    <Application Id="App" Executable="$($exeName.Name)" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="Local Knights" Square150x150Logo="Assets\Square150x150Logo.png" Square44x44Logo="Assets\Square44x44Logo.png" Description="Local Knights chess club manager" BackgroundColor="transparent" />
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

Set-Content -Path $manifestPath -Value $manifest -Encoding UTF8

# Pack with MakeAppx
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
$packagePath = Join-Path $OutputDir $PackageName

Write-Host "Packing MSIX to $packagePath using $makeappxExe..."
# Remove existing package if present to avoid interactive overwrite prompt
if (Test-Path $packagePath) {
  try { Remove-Item -Path $packagePath -Force -ErrorAction Stop; Write-Host "Removed existing package at $packagePath" } catch { Write-Warning "Could not remove existing package: $_" }
}
& $makeappxExe pack /d $publishDir /p $packagePath | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Error "MakeAppx failed."; exit 1 }

# Sign package
if (-not [string]::IsNullOrEmpty($CertificatePath)) {
    Write-Host "Signing package with provided certificate using $signtoolExe..."
    & $signtoolExe sign /fd SHA256 /f $CertificatePath /p $CertificatePassword /tr http://timestamp.digicert.com /td SHA256 $packagePath
} else {
    Write-Host "No certificate provided. Creating temporary self-signed certificate and signing package."
    # Use the PackagePublisher as the subject if it looks like a DN (e.g. 'CN=Name'), otherwise fall back
    $subject = if ($PackagePublisher -and $PackagePublisher -match "^\s*CN=") { $PackagePublisher } else { "CN=LocalKnightsTestCert" }
    $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject $subject -KeyExportPolicy Exportable -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddYears(5)
    $pfxPath = Join-Path $OutputDir "LocalKnightsTempCert.pfx"
    $pwd = [System.Guid]::NewGuid().ToString()
    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password (ConvertTo-SecureString -String $pwd -Force -AsPlainText)

    # Make certificate trusted for current user by copying it into TrustedPeople and TrustedRoot stores
    try {
      # Import the exported PFX into CurrentUser Trusted Root and TrustedPeople
      $securePwd = ConvertTo-SecureString -String $pwd -Force -AsPlainText
      Import-PfxCertificate -FilePath $pfxPath -CertStoreLocation Cert:\CurrentUser\Root -Password $securePwd -Exportable | Out-Null
      Import-PfxCertificate -FilePath $pfxPath -CertStoreLocation Cert:\CurrentUser\TrustedPeople -Password $securePwd -Exportable | Out-Null
      Write-Host "Imported temporary PFX into CurrentUser Trusted Root and Trusted People stores."
    }
    catch {
      Write-Warning "Failed to import temporary PFX into user stores: $_"
      Write-Host "You can manually install $pfxPath into Trusted Root Certification Authorities and Trusted People (Current User)."
    }

    & $signtoolExe sign /fd SHA256 /f $pfxPath /p $pwd /tr http://timestamp.digicert.com /td SHA256 $packagePath
    Write-Host "Temporary certificate exported to $pfxPath (delete if not needed)."

    if ($InstallToLocalMachine) {
      Write-Host "Preparing to import temporary certificate into LocalMachine stores (requires elevation)..."
      $helperScript = Join-Path $env:TEMP ("import_lm_{0}.ps1" -f ([Guid]::NewGuid().ToString()))
      $lmScript = @"
$securePwd = ConvertTo-SecureString -String '$pwd' -Force -AsPlainText
# Remove existing certs with same subject from LocalMachine stores
Get-ChildItem Cert:\LocalMachine\Root | Where-Object Subject -eq '$($cert.Subject)' | ForEach-Object { try { Remove-Item -Path ("Cert:\LocalMachine\Root\" + $_.Thumbprint) -Force -ErrorAction SilentlyContinue } catch {} }
Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object Subject -eq '$($cert.Subject)' | ForEach-Object { try { Remove-Item -Path ("Cert:\LocalMachine\TrustedPeople\" + $_.Thumbprint) -Force -ErrorAction SilentlyContinue } catch {} }
Import-PfxCertificate -FilePath '$pfxPath' -CertStoreLocation Cert:\LocalMachine\Root -Password $securePwd -Exportable | Out-Null
Import-PfxCertificate -FilePath '$pfxPath' -CertStoreLocation Cert:\LocalMachine\TrustedPeople -Password $securePwd -Exportable | Out-Null
Write-Host 'Imported PFX into LocalMachine Root and TrustedPeople stores.'
"@
      Set-Content -Path $helperScript -Value $lmScript -Encoding UTF8
      try {
        Start-Process -FilePath powershell -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-File',"$helperScript" -Verb runAs -Wait
        Remove-Item -Path $helperScript -Force -ErrorAction SilentlyContinue
        Write-Host "LocalMachine import helper completed."
      }
      catch {
        Write-Warning "Failed to run elevated import helper: $_"
        Write-Host "You can manually import $pfxPath into LocalMachine Root and TrustedPeople with admin rights."
      }
    }
}

Write-Host "MSIX package ready at: $packagePath"
Write-Host "Done."
