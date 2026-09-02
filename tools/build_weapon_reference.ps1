Add-Type -AssemblyName System.Drawing

$sources = @(
    'C:\Users\long0\AppData\Local\Temp\codex-clipboard-1eab2009-5b98-4c41-b217-c55c3437f595.png',
    'C:\Users\long0\AppData\Local\Temp\codex-clipboard-3d0006f7-616a-4801-9155-66ddbc8213f6.png',
    'C:\Users\long0\AppData\Local\Temp\codex-clipboard-4a58e08a-9534-474b-84bd-80cd730cb249.png',
    'C:\Users\long0\AppData\Local\Temp\codex-clipboard-72cea365-69fc-41ca-997e-8e29ef9e2cb1.png',
    'C:\Users\long0\AppData\Local\Temp\codex-clipboard-246604f8-7609-4cda-83e7-894c3c7412f6.png',
    'C:\Users\long0\AppData\Local\Temp\codex-clipboard-f3482ce7-fb82-439e-9c56-8ac461c380dc.png'
)

$output = Join-Path $PSScriptRoot 'weapon-reference-grid.png'
$canvas = New-Object System.Drawing.Bitmap 900, 600
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$graphics.Clear([System.Drawing.Color]::FromArgb(27, 43, 59))
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

for ($index = 0; $index -lt $sources.Count; $index++) {
    $image = [System.Drawing.Image]::FromFile($sources[$index])
    try {
        $column = $index % 3
        $row = [Math]::Floor($index / 3)
        $cellX = $column * 300
        $cellY = $row * 300
        $scale = [Math]::Min(260 / $image.Width, 245 / $image.Height)
        $width = [int]($image.Width * $scale)
        $height = [int]($image.Height * $scale)
        $x = $cellX + [int]((300 - $width) / 2)
        $y = $cellY + 42 + [int]((245 - $height) / 2)
        $graphics.DrawImage($image, $x, $y, $width, $height)
        $label = if ($row -eq 0) { "SWORD $($column + 1)" } else { "STAFF $($column + 1)" }
        $font = New-Object System.Drawing.Font('Arial', 18, [System.Drawing.FontStyle]::Bold)
        $brush = [System.Drawing.Brushes]::White
        $graphics.DrawString($label, $font, $brush, $cellX + 82, $cellY + 10)
        $font.Dispose()
    }
    finally {
        $image.Dispose()
    }
}

$canvas.Save($output, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$canvas.Dispose()
Write-Output $output
