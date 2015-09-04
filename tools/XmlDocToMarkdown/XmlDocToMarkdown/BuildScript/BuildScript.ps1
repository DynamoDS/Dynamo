Write-Host "Running Xml Markdown"

$app = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\XmlDocToMarkdown.exe"
$arg1 = "C:\projects\dynamo\bin\AnyCPU\Release\DynamoCore.dll"
$arg2 = "C:\projects\dynamo\bin\AnyCPU\Release\DynamoCore.xml"

If(-not(Test-Path -path $app))
  {
    $app
    Write-Host "File does not exists"	
  }
 
 If(-not(Test-Path -path $arg1))
  {
    $arg1
    Write-Host "File does not exists"	
  }
  
  If(-not(Test-Path -path $arg2))
  {
    $arg2
    Write-Host "File does not exists"	
  }
  
& $app $arg1 $arg2
Write-Host "XMLMarkdown ran successfully"

Write-Host "copying the markdown file"

$mdFile = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\index.md"
$docsDir = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\docs"

If(-not(Test-Path -path $docsDir))
  {  
	 $docsDir  
     Write-Host "File does not exists, Cannot copy item"	
  }
 Else
	{
		Copy-Item $mdFile  $docsDir
		Write-Host "Markdown file copied"
	}
	
Write-Host "Checking MkDocs"
Get-ChildItem -Path "C:\Python34" -Directory

  
