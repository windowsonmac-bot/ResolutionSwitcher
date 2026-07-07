<#
.SYNOPSIS
    Generates ResolutionSwitcher.Main/Resources/AppIcon.ico - a flat, retro CRT-monitor
    icon matching the app's Light theme palette (see ThemeManager.cs).

.DESCRIPTION
    Dev-time only tool. Not shipped, not referenced by the build besides the .ico it
    produces. Draws a simple blocky "retro monitor" glyph at 16/32/48/256 px using
    System.Drawing, then hand-assembles a multi-size .ico container (PNG-compressed
    frame for 256px per the modern ICO spec, raw DIB frames for the smaller sizes).

    Re-run this script any time the icon needs to be regenerated:
        powershell -File tools/AppIconGenerator.ps1
#>

Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'

$outputDir = Join-Path $PSScriptRoot '..\ResolutionSwitcher.Main\Resources'
$outputPath = Join-Path $outputDir 'AppIcon.ico'

if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Palette pulled from ThemeManager.cs Light theme (+ a couple of complementary accents).
$bezelColor  = [System.Drawing.ColorTranslator]::FromHtml('#ECE9D8') # FormBackground (Light)
$borderColor = [System.Drawing.ColorTranslator]::FromHtml('#ACA899') # muted taupe border
$screenColor = [System.Drawing.ColorTranslator]::FromHtml('#003C74') # TitleBarColor (Light)
$screenGlow  = [System.Drawing.ColorTranslator]::FromHtml('#5B9BD5') # lighter screen highlight
$ledColor    = [System.Drawing.ColorTranslator]::FromHtml('#3FA34D') # power LED green
$standColor  = [System.Drawing.ColorTranslator]::FromHtml('#D8D4C0') # slightly darker bezel tone

function Get-RoundedRectPath {
    param($Rect, [double]$Radius)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $Radius * 2
    $path.AddArc($Rect.X, $Rect.Y, $d, $d, 180, 90)
    $path.AddArc(($Rect.Right - $d), $Rect.Y, $d, $d, 270, 90)
    $path.AddArc(($Rect.Right - $d), ($Rect.Bottom - $d), $d, $d, 0, 90)
    $path.AddArc($Rect.X, ($Rect.Bottom - $d), $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function New-MonitorBitmap {
    param([int]$Size)

    $bmp = New-Object System.Drawing.Bitmap -ArgumentList @($Size, $Size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $s = $Size / 32.0 # base design grid is 32x32, scaled up

    # Monitor outer bezel
    $bezelRect = New-Object System.Drawing.RectangleF -ArgumentList @((2*$s), (2*$s), (28*$s), (20*$s))
    $bezelBrush = New-Object System.Drawing.SolidBrush -ArgumentList @(,$bezelColor)
    $penWidth = [Math]::Max(1.0, 1.2*$s)
    $borderPen = New-Object System.Drawing.Pen -ArgumentList @($borderColor, $penWidth)

    $bezelPath = Get-RoundedRectPath -Rect $bezelRect -Radius (2.5*$s)
    $g.FillPath($bezelBrush, $bezelPath)
    $g.DrawPath($borderPen, $bezelPath)

    # Screen (inset from bezel)
    $screenRect = New-Object System.Drawing.RectangleF -ArgumentList @((4.5*$s), (4.5*$s), (23*$s), (14*$s))
    $screenPath = Get-RoundedRectPath -Rect $screenRect -Radius (1.2*$s)
    $screenBrush = New-Object System.Drawing.SolidBrush -ArgumentList @(,$screenColor)
    $g.FillPath($screenBrush, $screenPath)

    # Screen glow highlight (diagonal band, top-left) for a little depth/character
    $glowPath = New-Object System.Drawing.Drawing2D.GraphicsPath
    $p1 = New-Object System.Drawing.PointF -ArgumentList @((5.5*$s), (5.5*$s))
    $p2 = New-Object System.Drawing.PointF -ArgumentList @((14*$s), (5.5*$s))
    $p3 = New-Object System.Drawing.PointF -ArgumentList @((7.5*$s), (13*$s))
    $p4 = New-Object System.Drawing.PointF -ArgumentList @((5.5*$s), (13*$s))
    $glowPoints = [System.Drawing.PointF[]]@($p1, $p2, $p3, $p4)
    $glowPath.AddPolygon($glowPoints)
    $glowColor = [System.Drawing.Color]::FromArgb(90, $screenGlow)
    $glowBrush = New-Object System.Drawing.SolidBrush -ArgumentList @(,$glowColor)
    $g.FillPath($glowBrush, $glowPath)

    # Stand neck
    $neckRect = New-Object System.Drawing.RectangleF -ArgumentList @((14*$s), (22*$s), (4*$s), (3.5*$s))
    $standBrush = New-Object System.Drawing.SolidBrush -ArgumentList @(,$standColor)
    $g.FillRectangle($standBrush, $neckRect)
    $g.DrawRectangle($borderPen, $neckRect.X, $neckRect.Y, $neckRect.Width, $neckRect.Height)

    # Stand base
    $baseRect = New-Object System.Drawing.RectangleF -ArgumentList @((10*$s), (25.5*$s), (12*$s), (2.6*$s))
    $basePath = Get-RoundedRectPath -Rect $baseRect -Radius (1.0*$s)
    $g.FillPath($standBrush, $basePath)
    $g.DrawPath($borderPen, $basePath)

    # Power LED
    $ledSize = [Math]::Max(1.5, 1.6*$s)
    $ledRect = New-Object System.Drawing.RectangleF -ArgumentList @((24.5*$s), (18*$s), $ledSize, $ledSize)
    $ledBrush = New-Object System.Drawing.SolidBrush -ArgumentList @(,$ledColor)
    $g.FillEllipse($ledBrush, $ledRect)

    $g.Dispose()
    return $bmp
}

function Get-PngBytes {
    param($Bitmap)
    $ms = New-Object System.IO.MemoryStream
    $Bitmap.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    return ,$ms.ToArray()
}

function Get-DibBytesForIco {
    param($Bitmap)
    # ICO "raw" frames use a DIB (BITMAPINFOHEADER + XOR + AND masks), not a full BMP file.
    # We build that manually since System.Drawing has no direct DIB export helper.
    $w = $Bitmap.Width
    $h = $Bitmap.Height
    $rowSize = [Math]::Ceiling(($w * 32) / 32.0) * 4
    $imageSize = $rowSize * $h
    $andRowSize = [Math]::Ceiling($w / 32.0) * 4
    $andSize = $andRowSize * $h

    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter -ArgumentList @(,$ms)

    # BITMAPINFOHEADER
    $bw.Write([UInt32]40)              # biSize
    $bw.Write([Int32]$w)               # biWidth
    $bw.Write([Int32]($h * 2))         # biHeight (XOR+AND combined per ICO spec)
    $bw.Write([UInt16]1)               # biPlanes
    $bw.Write([UInt16]32)              # biBitCount
    $bw.Write([UInt32]0)               # biCompression (BI_RGB)
    $bw.Write([UInt32]($imageSize + $andSize))
    $bw.Write([Int32]0)                # biXPelsPerMeter
    $bw.Write([Int32]0)                # biYPelsPerMeter
    $bw.Write([UInt32]0)               # biClrUsed
    $bw.Write([UInt32]0)               # biClrImportant

    # XOR mask: bottom-up, BGRA
    for ($y = $h - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $w; $x++) {
            $pixel = $Bitmap.GetPixel($x, $y)
            $bw.Write([byte]$pixel.B)
            $bw.Write([byte]$pixel.G)
            $bw.Write([byte]$pixel.R)
            $bw.Write([byte]$pixel.A)
        }
    }

    # AND mask: 1 bpp, all zero (fully opaque via alpha channel above)
    $andBytes = New-Object byte[] -ArgumentList @(,$andSize)
    $bw.Write($andBytes)

    $bw.Flush()
    return ,$ms.ToArray()
}

$sizes = @(16, 32, 48, 256)
$bitmaps = @{}
foreach ($size in $sizes) {
    $bitmaps[$size] = New-MonitorBitmap -Size $size
}

$entries = @()
foreach ($size in $sizes) {
    if ($size -eq 256) {
        $data = Get-PngBytes -Bitmap $bitmaps[$size]
    } else {
        $data = Get-DibBytesForIco -Bitmap $bitmaps[$size]
    }
    $entries += [PSCustomObject]@{ Size = $size; Data = $data }
}

$fs = New-Object System.IO.FileStream -ArgumentList @($outputPath, [System.IO.FileMode]::Create)
$bw = New-Object System.IO.BinaryWriter -ArgumentList @(,$fs)

# ICONDIR
$bw.Write([UInt16]0)               # reserved
$bw.Write([UInt16]1)               # type = icon
$bw.Write([UInt16]$entries.Count)  # image count

$headerSize = 6
$dirEntrySize = 16
$offset = $headerSize + ($dirEntrySize * $entries.Count)

foreach ($entry in $entries) {
    $sizeByte = if ($entry.Size -ge 256) { 0 } else { $entry.Size }
    $bw.Write([byte]$sizeByte)      # width (0 = 256)
    $bw.Write([byte]$sizeByte)      # height (0 = 256)
    $bw.Write([byte]0)              # color palette
    $bw.Write([byte]0)              # reserved
    $bw.Write([UInt16]1)            # color planes
    $bw.Write([UInt16]32)           # bits per pixel
    $bw.Write([UInt32]$entry.Data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $entry.Data.Length
}

foreach ($entry in $entries) {
    $bw.Write($entry.Data)
}

$bw.Flush()
$fs.Close()

foreach ($bmp in $bitmaps.Values) { $bmp.Dispose() }

Write-Host "Generated icon: $outputPath"
