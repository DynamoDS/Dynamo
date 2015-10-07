Write-Host "Copying the markdown file"
$mdFile = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\index.md"
$docsDir = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\docs"
If(-not(Test-Path -path $docsDir))
  {  
	 $docsDir  
     Write-Host "File does not exists, Cannot copy item"
     exit	 
  }
 Else
  {
	 Copy-Item $mdFile  $docsDir
	 Write-Host "Markdown file copied"
  } 
	
Write-Host "Installing MkDocs"
$pip = "C:\Python27\Scripts\pip.exe"
$pipArg1 = "install"
$pipArg2 ="mkdocs"

& $pip $pipArg1 $pipArg2
	
Write-Host "Copying MkDocs"
$mkDocs = "C:\Python27\Scripts\mkdocs.exe"
$mkDocsDest = "C:\projects\dynamo\DynamoAPI"

If(-not(Test-Path -path $mkDocsDest))
  {  
	 $mkDocsDest  
     Write-Host "File does not exists"
     exit	 
  }

Copy-Item $mkDocs  $mkDocsDest

Write-Host "Copying mkdocs.yml"
$ymlLocation = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\mkdocs.yml"

If(-not(Test-Path -path $ymlLocation))
  {  
	 $ymlLocation  
     Write-Host "File does not exists, Cannot copy item"
     exit	 
  }

Copy-Item $ymlLocation  $mkDocsDest

Write-Host "Copying the docs folder"
$docLocation = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\docs"

Copy-Item $docLocation  $mkDocsDest -recurse

Write-Host "Copying Themes folder"
$themeLoc = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\Theme"

If(-not(Test-Path -path $themeLoc))
  {  
	 $themeLoc  
     Write-Host "File does not exists, Cannot copy item"
     exit	 
  }

Copy-Item $themeLoc  $mkDocsDest -recurse

Write-Host "Running MkDocs"

Set-Location -Path $mkDocsDest
$mkDocsFile = "C:\projects\dynamo\DynamoAPI\mkDocs.exe"
$mkarg1 = "gh-deploy"
$mkarg2 = "--clean"

& $mkDocsFile $mkarg1 $mkarg2