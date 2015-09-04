Write-Host "Running Xml Markdown"

$app = "C:\projects\dynamo\tools\XmlDocToMarkdown\bin\Release\XmlDocToMarkdown.exe"
$arg1 = "C:\projects\dynamo\bin\Release\DynamoCore.dll"
$arg2 = "C:\projects\dynamo\bin\Release\DynamoCore.xml"

If(-not(Test-Path -path $app))
  {
    Write-Host "File does not exists on XmlDocToMarkdown "
  }
  
$check = "bin\Release\XmlDocToMarkdown.exe"

If(-not(Test-Path -path $check))
  {
    Write-Host "File does not exists on Dynamo"
  }

& $app $arg1 $arg2