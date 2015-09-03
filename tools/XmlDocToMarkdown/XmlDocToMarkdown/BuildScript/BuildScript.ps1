Write-Host "Running Xml Markdown"

$app = 'tools\XmlDocToMarkdown\XmlDocToMarkdown\bin\Release\XmlDocToMarkdown.exe'
$arg1 = '/bin/AnyCPU/Release/DynamoCore.dll'
$arg2 = '/bin/AnyCPU/Release/DynamoCore.xml'

& $app $arg1 $arg2