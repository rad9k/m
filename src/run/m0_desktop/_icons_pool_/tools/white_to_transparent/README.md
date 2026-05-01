# White To Transparent PNG Tool

Converts PNG files with a white background into PNG files with an alpha-transparent background.

The tool is intended for icon cleanup in `_icons_pool_`. It uses only Windows PowerShell/.NET `System.Drawing`, so it does not require Python packages.

## Usage

Convert one file:

```powershell
.\Convert-WhiteToTransparent.ps1 -SourcePath "..\flat\Attribute.png"
```

Convert one file to a selected output path:

```powershell
.\Convert-WhiteToTransparent.ps1 -SourcePath "..\flat\Attribute.png" -OutputPath ".\out\Attribute.png" -Overwrite
```

Convert all PNG files in a directory:

```powershell
.\Convert-WhiteToTransparent.ps1 -SourcePath "..\flat" -OutputPath ".\out" -Overwrite
```

Convert recursively:

```powershell
.\Convert-WhiteToTransparent.ps1 -SourcePath ".." -OutputPath ".\out" -Recurse -Overwrite
```

## Tuning

- `-WhiteThreshold` controls what counts as white. Default: `245`.
- `-Softness` controls edge feathering around near-white pixels. Default: `18`.
- `-Overwrite` is required when output files already exist.

