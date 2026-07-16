param(
    [Parameter(Mandatory = $true)]
    [string] $SourceDirectory
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceIcon = Join-Path $SourceDirectory 'CapX_App_Icon.png'
$sourceWordmark = Join-Path $SourceDirectory 'CapX_Primary_Dark.png'
$brandSource = Join-Path $repoRoot 'Brand\CapX\Source'

if (-not (Test-Path -LiteralPath $sourceIcon) -or -not (Test-Path -LiteralPath $sourceWordmark))
{
    throw 'CapX_App_Icon.png and CapX_Primary_Dark.png are required.'
}

New-Item -ItemType Directory -Force -Path $brandSource | Out-Null
Copy-Item -Path (Join-Path $SourceDirectory '*') -Destination $brandSource -Force

function New-SquareBitmap
{
    param([System.Drawing.Image] $Source, [int] $Size)

    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.DrawImage($Source, 0, 0, $Size, $Size)
    $graphics.Dispose()
    return $bitmap
}

function Save-SquarePng
{
    param([System.Drawing.Image] $Source, [int] $Size, [string] $Path)

    $bitmap = New-SquareBitmap -Source $Source -Size $Size
    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

function Get-PngBytes
{
    param([System.Drawing.Image] $Source, [int] $Size)

    $bitmap = New-SquareBitmap -Source $Source -Size $Size
    $stream = [System.IO.MemoryStream]::new()
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    $bytes = $stream.ToArray()
    $stream.Dispose()
    $bitmap.Dispose()
    return $bytes
}

function Save-MultiSizeIcon
{
    param([System.Drawing.Image] $Source, [string] $Path)

    $sizes = @(16, 32, 48, 64, 128, 256)
    $entries = foreach ($size in $sizes)
    {
        [pscustomobject]@{ Size = $size; Data = Get-PngBytes -Source $Source -Size $size }
    }

    $stream = [System.IO.File]::Create($Path)
    $writer = [System.IO.BinaryWriter]::new($stream)
    $writer.Write([uint16] 0)
    $writer.Write([uint16] 1)
    $writer.Write([uint16] $entries.Count)

    $offset = 6 + (16 * $entries.Count)
    foreach ($entry in $entries)
    {
        $dimension = if ($entry.Size -eq 256) { [byte] 0 } else { [byte] $entry.Size }
        $writer.Write($dimension)
        $writer.Write($dimension)
        $writer.Write([byte] 0)
        $writer.Write([byte] 0)
        $writer.Write([uint16] 1)
        $writer.Write([uint16] 32)
        $writer.Write([uint32] $entry.Data.Length)
        $writer.Write([uint32] $offset)
        $offset += $entry.Data.Length
    }

    foreach ($entry in $entries)
    {
        $writer.Write([byte[]] $entry.Data)
    }

    $writer.Dispose()
    $stream.Dispose()
}

function Save-AboutLogo
{
    param([System.Drawing.Image] $Source, [string] $Path)

    $bitmap = [System.Drawing.Bitmap]::new(400, 600, [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(10, 12, 18))
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.DrawImage($Source, 0, 100, 400, 400)
    $graphics.Dispose()
    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

$icon = [System.Drawing.Image]::FromFile($sourceIcon)
$wordmark = [System.Drawing.Image]::FromFile($sourceWordmark)

try
{
    $appResources = Join-Path $repoRoot 'ShareX\Resources'
    $helperResources = Join-Path $repoRoot 'ShareX.HelpersLib\Resources'
    $editorAssets = Join-Path $repoRoot 'ShareX.ImageEditor.App\Assets'

    Save-MultiSizeIcon -Source $icon -Path (Join-Path $appResources 'CapX_Icon.ico')
    Copy-Item -LiteralPath (Join-Path $appResources 'CapX_Icon.ico') -Destination (Join-Path $appResources 'CapX_File_Icon.ico') -Force
    Copy-Item -LiteralPath (Join-Path $appResources 'CapX_Icon.ico') -Destination (Join-Path $helperResources 'CapX_Icon.ico') -Force
    Copy-Item -LiteralPath (Join-Path $appResources 'CapX_Icon.ico') -Destination (Join-Path $editorAssets 'CapX_ImageEditor_Icon.ico') -Force

    Save-SquarePng -Source $icon -Size 16 -Path (Join-Path $appResources 'CapX_Icon_16.png')
    Save-SquarePng -Source $icon -Size 48 -Path (Join-Path $appResources 'CapX_Icon_48.png')
    Save-SquarePng -Source $icon -Size 256 -Path (Join-Path $helperResources 'CapX_Logo.png')
    Save-AboutLogo -Source $wordmark -Path (Join-Path $appResources 'CapX_About_Logo.png')

    $storeAssets = Join-Path $repoRoot 'ShareX.Setup\MicrosoftStore\Assets'
    Get-ChildItem -LiteralPath $storeAssets -Filter '*.png' | ForEach-Object {
        $existing = [System.Drawing.Image]::FromFile($_.FullName)
        $width = $existing.Width
        $height = $existing.Height
        $existing.Dispose()

        $bitmap = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

        $size = [Math]::Min($width, $height)
        $left = [int](($width - $size) / 2)
        $top = [int](($height - $size) / 2)
        $graphics.DrawImage($icon, $left, $top, $size, $size)
        $graphics.Dispose()
        $bitmap.Save($_.FullName, [System.Drawing.Imaging.ImageFormat]::Png)
        $bitmap.Dispose()
    }
}
finally
{
    $icon.Dispose()
    $wordmark.Dispose()
}

Write-Host 'CapX assets generated successfully.'
