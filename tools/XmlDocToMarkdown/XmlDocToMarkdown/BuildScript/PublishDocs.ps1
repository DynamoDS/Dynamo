Write-Host "Install MkDocs"
$pip = "C:\Python27\Scripts\pip.exe"
$pipArg1 = "install"
$pipArg2 ="mkdocs"

& $pip $pipArg1 $pipArg2
	
Write-Host "Copying MkDocs"
$mkDocs = "C:\Python27\Scripts\mkdocs.exe"
$mkDocsDest = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release"

Copy-Item $mkDocs  $mkDocsDest

Write-Host "Running MkDocs"
$mkDocsLoc = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release"

Set-Location -Path $mkDocsLoc
Get-Location
$mkDocsFile = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\mkDocs.exe"
$mkarg1 = "gh-deploy"
$mkarg2 = "--remote-branch"
$mkarg3 = "gh-pages"

& $mkDocsFile $mkarg1 $mkarg2 $mkarg3