param(
    [string]$SourcePath = ".",

    [string]$OutputPath,

    [int]$WhiteThreshold = 245,

    [int]$Softness = 18,

    [switch]$Recurse,

    [switch]$Overwrite
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

function Get-FullPath {
    param([string]$Path)

    return [System.IO.Path]::GetFullPath($Path)
}

function Get-OutputFilePath {
    param(
        [System.IO.FileInfo]$SourceFile,
        [string]$OutputRoot
    )

    if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
        return [System.IO.Path]::Combine(
            $SourceFile.DirectoryName,
            [System.IO.Path]::GetFileNameWithoutExtension($SourceFile.Name) + "_transparent.png")
    }

    $fullOutputRoot = Get-FullPath $OutputRoot

    if ([System.IO.Directory]::Exists($fullOutputRoot) -or $OutputRoot.EndsWith("\") -or $OutputRoot.EndsWith("/")) {
        [System.IO.Directory]::CreateDirectory($fullOutputRoot) | Out-Null
        return [System.IO.Path]::Combine($fullOutputRoot, $SourceFile.Name)
    }

    $outputDirectory = [System.IO.Path]::GetDirectoryName($fullOutputRoot)

    if ([string]::IsNullOrWhiteSpace($outputDirectory) -eq $false) {
        [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
    }

    return $fullOutputRoot
}

function Get-RelativePath {
    param(
        [string]$BasePath,
        [string]$FullPath
    )

    $fullBasePath = Get-FullPath $BasePath

    if ($fullBasePath.EndsWith([System.IO.Path]::DirectorySeparatorChar) -eq $false) {
        $fullBasePath += [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = New-Object System.Uri($fullBasePath)
    $fullUri = New-Object System.Uri((Get-FullPath $FullPath))
    $relativeUri = $baseUri.MakeRelativeUri($fullUri)

    return [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace("/", [System.IO.Path]::DirectorySeparatorChar)
}

function Convert-WhiteBackgroundToTransparent {
    param(
        [string]$InputFilePath,
        [string]$OutputFilePath,
        [int]$Threshold,
        [int]$Feather,
        [bool]$AllowOverwrite
    )

    if ([System.IO.File]::Exists($OutputFilePath) -and $AllowOverwrite -eq $false) {
        throw "Output file already exists: $OutputFilePath. Use -Overwrite to replace it."
    }

    $outputDirectory = [System.IO.Path]::GetDirectoryName($OutputFilePath)

    if ([string]::IsNullOrWhiteSpace($outputDirectory) -eq $false) {
        [System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
    }

    $sourceBitmap = [System.Drawing.Bitmap]::FromFile($InputFilePath)

    try {
        $outputBitmap = New-Object System.Drawing.Bitmap `
            $sourceBitmap.Width, `
            $sourceBitmap.Height, `
            ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

        try {
            $innerDistance = [Math]::Max(0, 255 - $Threshold)
            $outerDistance = $innerDistance + [Math]::Max(1, $Feather)

            for ($y = 0; $y -lt $sourceBitmap.Height; $y++) {
                for ($x = 0; $x -lt $sourceBitmap.Width; $x++) {
                    $pixel = $sourceBitmap.GetPixel($x, $y)
                    $distanceFromWhite = [Math]::Sqrt(
                        [Math]::Pow(255 - $pixel.R, 2) +
                        [Math]::Pow(255 - $pixel.G, 2) +
                        [Math]::Pow(255 - $pixel.B, 2))

                    if ($distanceFromWhite -le $innerDistance) {
                        $alphaFactor = 0.0
                    }
                    elseif ($distanceFromWhite -ge $outerDistance) {
                        $alphaFactor = 1.0
                    }
                    else {
                        $alphaFactor = ($distanceFromWhite - $innerDistance) / ($outerDistance - $innerDistance)
                    }

                    $newAlpha = [int][Math]::Round($pixel.A * $alphaFactor)

                    if ($newAlpha -le 0) {
                        $outputPixel = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
                    }
                    elseif ($newAlpha -ge 255) {
                        $outputPixel = [System.Drawing.Color]::FromArgb($pixel.A, $pixel.R, $pixel.G, $pixel.B)
                    }
                    else {
                        $alpha = $newAlpha / 255.0
                        $red = [Math]::Max(0, [Math]::Min(255, [Math]::Round(($pixel.R - 255 * (1 - $alpha)) / $alpha)))
                        $green = [Math]::Max(0, [Math]::Min(255, [Math]::Round(($pixel.G - 255 * (1 - $alpha)) / $alpha)))
                        $blue = [Math]::Max(0, [Math]::Min(255, [Math]::Round(($pixel.B - 255 * (1 - $alpha)) / $alpha)))

                        $outputPixel = [System.Drawing.Color]::FromArgb($newAlpha, $red, $green, $blue)
                    }

                    $outputBitmap.SetPixel($x, $y, $outputPixel)
                }
            }

            $outputBitmap.Save($OutputFilePath, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $outputBitmap.Dispose()
        }
    }
    finally {
        $sourceBitmap.Dispose()
    }
}

$fullSourcePath = Get-FullPath $SourcePath

if ([System.IO.File]::Exists($fullSourcePath)) {
    $sourceFile = Get-Item -LiteralPath $fullSourcePath
    $outputFilePath = Get-OutputFilePath $sourceFile $OutputPath

    Write-Host "Converting: $($sourceFile.FullName)"

    Convert-WhiteBackgroundToTransparent `
        -InputFilePath $sourceFile.FullName `
        -OutputFilePath $outputFilePath `
        -Threshold $WhiteThreshold `
        -Feather $Softness `
        -AllowOverwrite $Overwrite.IsPresent

    Write-Host "Converted: $($sourceFile.FullName) -> $outputFilePath"
    exit 0
}

if ([System.IO.Directory]::Exists($fullSourcePath)) {
    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        $OutputPath = [System.IO.Path]::Combine($fullSourcePath, "transparent")
    }

    $searchOption = if ($Recurse.IsPresent) {
        [System.IO.SearchOption]::AllDirectories
    }
    else {
        [System.IO.SearchOption]::TopDirectoryOnly
    }

    $sourceFiles = @([System.IO.Directory]::GetFiles($fullSourcePath, "*.png", $searchOption) | Sort-Object)
    $convertedCount = 0

    if ($sourceFiles.Count -eq 0) {
        Write-Host "No PNG files found in: $fullSourcePath"
        exit 0
    }

    foreach ($sourceFilePath in $sourceFiles) {
        $sourceFile = Get-Item -LiteralPath $sourceFilePath
        $relativePath = Get-RelativePath $fullSourcePath $sourceFile.FullName
        $outputFilePath = [System.IO.Path]::Combine((Get-FullPath $OutputPath), $relativePath)

        Write-Host "Converting: $relativePath"

        Convert-WhiteBackgroundToTransparent `
            -InputFilePath $sourceFile.FullName `
            -OutputFilePath $outputFilePath `
            -Threshold $WhiteThreshold `
            -Feather $Softness `
            -AllowOverwrite $Overwrite.IsPresent

        $convertedCount++
        Write-Host "Converted: $relativePath"
    }

    Write-Host "Done. Converted $convertedCount PNG file(s)."
    exit 0
}

throw "SourcePath does not exist: $SourcePath"
