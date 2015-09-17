Write-Host "Install MkDocs"
$pip = "C:\Python27\Scripts\pip.exe"
$pipArg1 = "install"
$pipArg2 ="mkdocs"

& $pip $pipArg1 $pipArg2
	
Write-Host "Copying MkDocs"
$mkDocs = "C:\Python27\Scripts\mkdocs.exe"
$mkDocsDest = "C:\projects\dynamo\DocumentTesting"

Copy-Item $mkDocs  $mkDocsDest

Write-Host "Copying yml"
$ymlLocation = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\mkdocs.yml"
$ymlDest = "C:\projects\dynamo\DocumentTesting"

Copy-Item $ymlLocation  $ymlDest

Write-Host "Copying Docs folder"
$docLocation = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\docs"
$docDest = "C:\projects\dynamo\DocumentTesting"

Copy-Item $docLocation  $docDest -recurse

Write-Host "Copying Themes folder"
$themeLoc = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\Theme"
$themeDest = "C:\projects\dynamo\DocumentTesting"

Copy-Item $themeLoc  $themeDest -recurse

Write-Host "Running MkDocs"
$mkDocsLoc = "C:\projects\dynamo\DocumentTesting"

Set-Location -Path $mkDocsLoc
$mkDocsFile = "C:\projects\dynamo\DocumentTesting\mkDocs.exe"
$mkarg1 = "gh-deploy"
$mkarg2 = "--clean"

& $mkDocsFile $mkarg1 $mkarg2