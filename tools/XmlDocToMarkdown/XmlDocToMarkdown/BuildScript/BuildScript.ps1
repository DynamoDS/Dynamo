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



  
