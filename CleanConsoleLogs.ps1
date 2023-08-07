<#
   Purpose: Clean empty files
#>
$ErrorActionPreference = "Stop"

$SysOut = "//system-out"
$SysErr = "//system-err"

Get-ChildItem "$env:WORKSPACE\TestResults" -Filter *.xml | 
Foreach-Object {
    $Path = $_.FullName
    [xml]$xml = Get-Content $Path

    $nodes = $xml.SelectNodes("$SysOut")
    foreach($Child in $nodes){
        [void]$Child.ParentNode.RemoveChild($Child)
    }
    $nodes = $xml.SelectNodes("$SysErr")
    foreach($Child in $nodes){
        [void]$Child.ParentNode.RemoveChild($Child)
    }

    $xml.Save("$Path")
}
