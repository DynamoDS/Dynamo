Write-Host "Running Xml Markdown"

$app = "C:\projects\dynamo\tools\XmlDocToMarkdown\bin\Release\XmlDocToMarkdown.exe"
$arg1 = "C:\projects\dynamo\bin\Release\DynamoCore.dll"
$arg2 = "C:\projects\dynamo\bin\Release\DynamoCore.xml"

If(-not(Test-Path -path $app))
  {
    Write-Host "File does not exists on XmlDocToMarkdown "
	Write-Host "Dynamo Path"		 
	Get-ChildItem -Path C:\projects\dynamo\  –Directory
	Write-Host "Dynamo Path1"
	Get-ChildItem -Path C:\projects\dynamo\bin\AnyCPU\ –Directory
	Write-Host "Dynamo Path2"
	Get-ChildItem -Path C:\projects\dynamo\bin\AnyCPU\Release\ –File
	Write-Host "Dynamo Path3"
	Get-ChildItem -Path C:\projects\dynamo\tools -Directory
	Write-Host "Dynamo Path4"
	Get-ChildItem -Path C:\projects\dynamo\tools\XmlDocToMarkdown\bin -Directory
	Write-Host "Dynamo Path5"
	Get-ChildItem -Path C:\projects\dynamo\tools\XmlDocToMarkdown\bin\AnyCPU\Release -File
  }
  
$check = "bin\Release\XmlDocToMarkdown.exe"

If(-not(Test-Path -path $arg1))
  {
    Write-Host "File does not exists on Dynamo"
  }

& $app $arg1 $arg2