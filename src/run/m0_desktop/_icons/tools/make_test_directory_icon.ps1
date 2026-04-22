# Generates a single clean 256x256 RGBA PNG with straight (non-premultiplied) alpha.
# Corners are fully transparent (A=0), glyph is fully opaque (A=255). Standard PNG format.
# Output (both get the same 256x256 straight-alpha PNG):
#   m0_desktop\_icons\test\Directory.png   — versioned
#   <bin>\icons\test\Directory.png         — app runtime (Debug)

param(
    [string]$VersionedPath = "m0_desktop\_icons\test\Directory.png",
    [string]$OutputPath = "m0_desktop\bin\Debug\net10.0-windows7.0\icons\test\Directory.png"
)

Add-Type -AssemblyName System.Drawing

$size = 256

$bitmap = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.Clear([System.Drawing.Color]::Transparent)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

$fillColor = [System.Drawing.Color]::FromArgb(255, 30, 106, 250)
$brush = New-Object System.Drawing.SolidBrush $fillColor

$tabPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$tabPath.AddPolygon([System.Drawing.PointF[]] @(
    (New-Object System.Drawing.PointF 32, 70),
    (New-Object System.Drawing.PointF 104, 70),
    (New-Object System.Drawing.PointF 124, 96),
    (New-Object System.Drawing.PointF 32, 96)
))
$graphics.FillPath($brush, $tabPath)
$tabPath.Dispose()

$bodyRect = New-Object System.Drawing.RectangleF 32, 88, 192, 118
$cornerRadius = 12.0
$bodyPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$bodyPath.AddArc($bodyRect.X, $bodyRect.Y, $cornerRadius * 2, $cornerRadius * 2, 180, 90)
$bodyPath.AddArc($bodyRect.Right - $cornerRadius * 2, $bodyRect.Y, $cornerRadius * 2, $cornerRadius * 2, 270, 90)
$bodyPath.AddArc($bodyRect.Right - $cornerRadius * 2, $bodyRect.Bottom - $cornerRadius * 2, $cornerRadius * 2, $cornerRadius * 2, 0, 90)
$bodyPath.AddArc($bodyRect.X, $bodyRect.Bottom - $cornerRadius * 2, $cornerRadius * 2, $cornerRadius * 2, 90, 90)
$bodyPath.CloseFigure()
$graphics.FillPath($brush, $bodyPath)
$bodyPath.Dispose()

$brush.Dispose()
$graphics.Dispose()

function Save-TestDirectoryPng {
    param([string]$Path)
    $full = [System.IO.Path]::GetFullPath($Path)
    $dir = [System.IO.Path]::GetDirectoryName($full)
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    $bitmap.Save($full, [System.Drawing.Imaging.ImageFormat]::Png)
    "Saved: $full"
}

Save-TestDirectoryPng -Path $VersionedPath
Save-TestDirectoryPng -Path $OutputPath
$bitmap.Dispose()
