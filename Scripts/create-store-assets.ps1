<#
.SYNOPSIS
    Generates Microsoft Store listing artwork from the Local Knights logo.

.DESCRIPTION
    Creates the PNG dimensions shown in Partner Center's Store logos, Store
    display images, Windows and Xbox image, and Xbox images sections.
#>
param(
    [string]$SourceDir = (Join-Path $PSScriptRoot "..\LocalKnights.WindowsDesktop\Resources\Icon"),
    [string]$OutputDir = (Join-Path $PSScriptRoot "..\Docs\StoreAssets"),
    [uri]$FontUri = "https://fonts.gstatic.com/s/spacegrotesk/v22/V8mQoQDjQSkFtoMM3T6r8E7mF71Q-gOoraIAEj4PVksj.ttf"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

function New-Canvas {
    param([int]$Width, [int]$Height)

    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

    $bounds = New-Object System.Drawing.Rectangle(0, 0, $Width, $Height)
    $gradient = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $bounds,
        [System.Drawing.Color]::FromArgb(255, 9, 29, 51),
        [System.Drawing.Color]::FromArgb(255, 20, 67, 105),
        35
    )
    $graphics.FillRectangle($gradient, $bounds)
    $gradient.Dispose()

    $glowBounds = New-Object System.Drawing.Rectangle(
        [int]($Width * 0.18),
        [int]($Height * 0.12),
        [int]($Width * 0.64),
        [int]($Height * 0.72)
    )
    $glowPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $glowPath.AddEllipse($glowBounds)
    $glow = New-Object System.Drawing.Drawing2D.PathGradientBrush($glowPath)
    $glow.CenterColor = [System.Drawing.Color]::FromArgb(48, 78, 151, 197)
    $glow.SurroundColors = [System.Drawing.Color[]]@([System.Drawing.Color]::FromArgb(0, 78, 151, 197))
    $graphics.FillEllipse($glow, $glowBounds)
    $glow.Dispose()
    $glowPath.Dispose()

    return @{
        Bitmap = $bitmap
        Graphics = $graphics
        Width = $Width
        Height = $Height
    }
}

function Add-BoardPattern {
    param($Canvas, [double]$TopRatio, [double]$CellRatio, [int]$Opacity)

    $graphics = $Canvas.Graphics
    $cell = [Math]::Max(2, [int]($Canvas.Width * $CellRatio))
    $top = [int]($Canvas.Height * $TopRatio)
    $rows = [Math]::Ceiling(($Canvas.Height - $top) / $cell)
    $columns = [Math]::Ceiling($Canvas.Width / $cell)
    $light = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb($Opacity, 96, 174, 210))
    $dark = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb([int]($Opacity * 0.38), 5, 19, 34))

    for ($row = 0; $row -lt $rows; $row++) {
        for ($column = 0; $column -lt $columns; $column++) {
            $brush = if (($row + $column) % 2 -eq 0) { $light } else { $dark }
            $graphics.FillRectangle($brush, $column * $cell, $top + $row * $cell, $cell, $cell)
        }
    }

    $light.Dispose()
    $dark.Dispose()
}

function Draw-Logo {
    param($Canvas, [System.Drawing.Image]$Logo, [int]$X, [int]$Y, [int]$Size)

    $Canvas.Graphics.DrawImage($Logo, $X, $Y, $Size, $Size)
}

function Draw-Title {
    param($Canvas, [int]$Y, [int]$FontSize, [switch]$Stacked)

    $font = New-Object System.Drawing.Font($script:SpaceGroteskFamily, $FontSize, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
    $localBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 246, 249, 252))
    $knightsBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 244, 153, 79))
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $lineHeight = [int]($FontSize * 1.24)

    if ($Stacked) {
        $localRect = New-Object System.Drawing.RectangleF(0, $Y, $Canvas.Width, $lineHeight)
        $knightsRect = New-Object System.Drawing.RectangleF(0, ($Y + $lineHeight), $Canvas.Width, $lineHeight)
        $Canvas.Graphics.DrawString("Local", $font, $localBrush, $localRect, $format)
        $Canvas.Graphics.DrawString("Knights", $font, $knightsBrush, $knightsRect, $format)
    }
    else {
        $localSize = $Canvas.Graphics.MeasureString("Local", $font)
        $spaceSize = $Canvas.Graphics.MeasureString(" ", $font)
        $knightsSize = $Canvas.Graphics.MeasureString("Knights", $font)
        $totalWidth = $localSize.Width + $spaceSize.Width + $knightsSize.Width
        $x = [single](($Canvas.Width - $totalWidth) / 2)
        $textY = [single]($Y + (($lineHeight - $localSize.Height) / 2))
        $Canvas.Graphics.DrawString("Local", $font, $localBrush, $x, $textY)
        $x += $localSize.Width
        $Canvas.Graphics.DrawString(" ", $font, $localBrush, $x, $textY)
        $x += $spaceSize.Width
        $Canvas.Graphics.DrawString("Knights", $font, $knightsBrush, $x, $textY)
    }

    $format.Dispose()
    $localBrush.Dispose()
    $knightsBrush.Dispose()
    $font.Dispose()
}

function Save-Canvas {
    param($Canvas, [string]$FileName)

    $path = Join-Path $OutputDir $FileName
    $Canvas.Bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $Canvas.Graphics.Dispose()
    $Canvas.Bitmap.Dispose()
    Write-Host "Created $path"
}

function New-StoreLogo {
    param(
        [int]$Width,
        [int]$Height,
        [string]$FileName,
        [System.Drawing.Image]$Logo,
        [bool]$ShowAccentLine = $true
    )

    $canvas = New-Canvas -Width $Width -Height $Height
    Add-BoardPattern -Canvas $canvas -TopRatio 0.76 -CellRatio 0.055 -Opacity 16
    $logoSize = [int]([Math]::Min($Width, $Height) * 0.64)
    Draw-Logo -Canvas $canvas -Logo $Logo -X ([int](($Width - $logoSize) / 2)) -Y ([int]($Height * 0.24)) -Size $logoSize
    if ($ShowAccentLine) {
        $accent = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(220, 244, 137, 58), [Math]::Max(3, [int]($Width * 0.004)))
        $canvas.Graphics.DrawLine($accent, [int]($Width * 0.34), [int]($Height * 0.79), [int]($Width * 0.66), [int]($Height * 0.79))
        $accent.Dispose()
    }
    Draw-Title -Canvas $canvas -Y ([int]($Height * 0.82)) -FontSize ([int]($Width * 0.047))
    Save-Canvas -Canvas $canvas -FileName $FileName
}

$sourcePath = [System.IO.Path]::GetFullPath($SourceDir)
$outputPath = [System.IO.Path]::GetFullPath($OutputDir)
$transparentLogoPath = Join-Path $sourcePath "logo.png"
$appIconPath = Join-Path $sourcePath "logo-bg.png"
foreach ($inputPath in @($transparentLogoPath, $appIconPath)) {
    if (-not (Test-Path -LiteralPath $inputPath -PathType Leaf)) {
        throw "Required source image is missing: '$inputPath'."
    }
}

New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
$fontTempPath = Join-Path $env:TEMP ("SpaceGrotesk-Bold_{0}.ttf" -f [Guid]::NewGuid().ToString("N"))
$script:SpaceGroteskFontCollection = $null
$logo = $null
$appIcon = $null

try {
    if ($FontUri.Scheme -ne "https") {
        throw "The Space Grotesk font URL must use HTTPS."
    }

    Write-Host "Downloading Space Grotesk font from $FontUri..."
    Invoke-WebRequest -Uri $FontUri -OutFile $fontTempPath -UseBasicParsing
    if ((Get-Item -LiteralPath $fontTempPath).Length -lt 10000) {
        throw "Downloaded Space Grotesk font file is unexpectedly small."
    }

    $script:SpaceGroteskFontCollection = New-Object System.Drawing.Text.PrivateFontCollection
    $script:SpaceGroteskFontCollection.AddFontFile($fontTempPath)
    if ($script:SpaceGroteskFontCollection.Families.Count -eq 0) {
        throw "Downloaded font did not contain an installable font family."
    }
    $script:SpaceGroteskFamily = $script:SpaceGroteskFontCollection.Families[0]
    $logo = [System.Drawing.Image]::FromFile($transparentLogoPath)
    $appIcon = [System.Drawing.Image]::FromFile($appIconPath)

    New-StoreLogo -Width 1440 -Height 2160 -FileName "StoreLogo-Poster-1440x2160.png" -Logo $logo
    New-StoreLogo -Width 2160 -Height 2160 -FileName "StoreLogo-BoxArt-2160x2160.png" -Logo $logo -ShowAccentLine $false

    foreach ($size in @(300, 150, 71)) {
        $tile = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($tile)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.DrawImage($appIcon, 0, 0, $size, $size)
        $tile.Save((Join-Path $outputPath "StoreDisplay-AppTile-$($size)x$($size).png"), [System.Drawing.Imaging.ImageFormat]::Png)
        $graphics.Dispose()
        $tile.Dispose()
    }

    $hero = New-Canvas -Width 3840 -Height 2160
    Add-BoardPattern -Canvas $hero -TopRatio 0.72 -CellRatio 0.035 -Opacity 19
    Draw-Logo -Canvas $hero -Logo $logo -X 1380 -Y 270 -Size 1080
    Save-Canvas -Canvas $hero -FileName "WindowsXbox-SuperHero-3840x2160.png"

    $keyArt = New-Canvas -Width 584 -Height 800
    Add-BoardPattern -Canvas $keyArt -TopRatio 0.72 -CellRatio 0.09 -Opacity 20
    Draw-Title -Canvas $keyArt -Y 64 -FontSize 49 -Stacked
    Draw-Logo -Canvas $keyArt -Logo $logo -X 77 -Y 228 -Size 430
    Save-Canvas -Canvas $keyArt -FileName "Xbox-BrandedKeyArt-584x800.png"

    $titledHero = New-Canvas -Width 1920 -Height 1080
    Add-BoardPattern -Canvas $titledHero -TopRatio 0.72 -CellRatio 0.055 -Opacity 18
    Draw-Title -Canvas $titledHero -Y 100 -FontSize 112
    Draw-Logo -Canvas $titledHero -Logo $logo -X 660 -Y 280 -Size 600
    Save-Canvas -Canvas $titledHero -FileName "Xbox-TitledHero-1920x1080.png"

    $promo = New-Canvas -Width 1080 -Height 1080
    Add-BoardPattern -Canvas $promo -TopRatio 0.78 -CellRatio 0.075 -Opacity 18
    Draw-Logo -Canvas $promo -Logo $logo -X 250 -Y 250 -Size 580
    Save-Canvas -Canvas $promo -FileName "Xbox-FeaturedPromotionalSquare-1080x1080.png"
}
finally {
    if ($logo) { $logo.Dispose() }
    if ($appIcon) { $appIcon.Dispose() }
    if ($script:SpaceGroteskFontCollection) { $script:SpaceGroteskFontCollection.Dispose() }
    if ($fontTempPath -and (Test-Path -LiteralPath $fontTempPath)) {
        Remove-Item -LiteralPath $fontTempPath -Force
    }
}

Write-Host "Store artwork generated in $outputPath"
