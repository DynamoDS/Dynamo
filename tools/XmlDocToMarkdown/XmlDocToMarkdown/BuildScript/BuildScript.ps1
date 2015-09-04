Write-Host "Running Xml Markdown"

$app = "C:\projects\dynamo\tools\XmlDocToMarkdown\bin\Release\XmlDocToMarkdown.exe"
$arg1 = "C:\projects\dynamo\bin\Release\DynamoCore.dll"
$arg2 = "C:\projects\dynamo\bin\Release\DynamoCore.xml"

If(-not(Test-Path -path $app))
  {
    Write-Host "File does not exists on XmlDocToMarkdown "
	Write-Host "Root Path"
	
	resolve-path ~
	
	resolve-path APPVEYOR_BUILD_FOLDER	
	resolve-path bin\Release\DynamoCore.dll
	resolve-path bin\Release\DynamoCore.xml
	resolve-path tools\XmlDocToMarkdown\bin\Release\XmlDocToMarkdown.exe
	
  }
  
$check = "bin\Release\XmlDocToMarkdown.exe"

If(-not(Test-Path -path $arg1))
  {
    Write-Host "File does not exists on Dynamo"
  }

& $app $arg1 $arg2