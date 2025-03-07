# Check missing node help docs
# https://github.com/DynamoDS/Dynamo/tree/master/doc/distrib/NodeHelpFiles
param(
    [Parameter(Mandatory = $true)][string]$path
)
$ErrorActionPreference = "Stop"

$help_files = Get-ChildItem -Path $path -Recurse -Include *.md
$image_files = Get-ChildItem -Path $path -Recurse -Include *.png, *.jpg, *.gif
$example_files = Get-ChildItem -Path $path -Recurse -Include *.dyn

Write-Output "Help files: $($help_files.Count)"
Write-Output "Image files: $($image_files.Count)"
Write-Output "Example files: $($example_files.Count)"

$help_files_without_image_files = @()
$help_files_without_example_files = @()
$image_files_without_help_files = @()
$example_files_without_help_files = @()

# Help files
foreach ($file in $help_files) {
    $base_name = $file.BaseName
    $png_file = Join-Path $path -ChildPath "$($base_name).png"
    $img_png_file = Join-Path $path -ChildPath "$($base_name)_img.png"
    $img_jpg_file = Join-Path $path -ChildPath "$($base_name)_img.jpg"
    $img_gif_file = Join-Path $path -ChildPath "$($base_name)_img.gif"
    $dyn_file = Join-Path $path -ChildPath "$($base_name).dyn"

    if (-Not ((Test-Path $png_file) -Or (Test-Path $img_png_file) -Or (Test-Path $img_jpg_file) -Or (Test-Path $img_gif_file))) {
        $help_files_without_image_files += $file
    }

    if (-Not (Test-path $dyn_file)) {
        $help_files_without_example_files += $file
    }
}

# Help files without image files 
Write-Output "`nHelp file without image files: $($help_files_without_image_files.Count)"
if ($help_files_without_image_files.Count -gt 0) {
    foreach ($file in $help_files_without_image_files) {
        Write-Output $file.Name
    }
}

# Help files without example files
Write-Output "`nHelp file without example files: $($help_files_without_example_files.Count)"
if ($help_files_without_example_files.Count -gt 0) {
    foreach ($file in $help_files_without_example_files) {
        Write-Output $file.Name
    }
}

# Image files
foreach ($file in $image_files) {
    $base_name = $file.BaseName -replace "_img", ""
    $dyn_file = Join-Path $path -ChildPath "$($base_name).dyn"

    if (-Not (Test-path $dyn_file)) {
        $image_files_without_help_files += $file
    }
}

# Image files without help files
Write-Output "`nImage file without help files: $($image_files_without_help_files.Count)"
if ($help_files_without_example_files.Count -gt 0) {
    foreach ($file in $image_files_without_help_files) {
        Write-Output $file.FullName
    }
}

# Example files
foreach ($file in $example_files) {
    $base_name = $file.BaseName
    $dyn_file = Join-Path $path -ChildPath "$($base_name).dyn"

    if (-Not (Test-path $dyn_file)) {
        $example_files_without_help_files += $file
    }
}

# Example files without help files
Write-Output "`nExample file without help files: $($example_files_without_help_files.Count)"
if ($example_files_without_help_files.Count -gt 0) {
    foreach ($file in $example_files_without_help_files) {
        Write-Output $file.FullName
    }
}

Start-Sleep -Seconds 30