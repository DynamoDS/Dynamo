Write-Host "Running Xml Markdown"

$app = "C:\projects\dynamo\tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\AnyCPU\Release\XmlDocToMarkdown.exe"
$arg1 = "C:\projects\dynamo\bin\AnyCPU\Release\DynamoCore.dll"
$arg2 = "C:\projects\dynamo\bin\AnyCPU\Release\DynamoCore.xml"

Write-Host "Dynamo Path3"
Get-ChildItem -Path C:\projects\dynamo\bin\AnyCPU\Release -File

Write-Host "Dynamo Path4"
Get-ChildItem -Path C:\projects\dynamo\tools\XmlDocToMarkdown -Directory

If(-not(Test-Path -path $app))
  {
    $app
    Write-Host "File does not exists"
	{ "File does not exists" ; exit }
  }
 
 If(-not(Test-Path -path $arg1))
  {
    $arg1
    Write-Host "File does not exists"
	{ "File does not exists" ; exit }
  }
  
  If(-not(Test-Path -path $arg2))
  {
    $arg2
    Write-Host "File does not exists"
	{ "File does not exists" ; exit }
  }
  

& $app $arg1 $arg2