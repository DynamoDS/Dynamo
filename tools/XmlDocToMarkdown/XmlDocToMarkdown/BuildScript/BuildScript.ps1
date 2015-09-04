Write-Host "Running Xml Markdown"

$app = "C:\projects\dynamo\tools\XmlDocToMarkdown\bin\Release\XmlDocToMarkdown.exe"
$arg1 = "C:\projects\dynamo\bin\Release\DynamoCore.dll"
$arg2 = "C:\projects\dynamo\bin\Release\DynamoCore.xml"

& $app $arg1 $arg2