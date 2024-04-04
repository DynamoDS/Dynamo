param([int]$issueNumber)

$issueTitle = $env:ISSUE_TITLE
$issueDescription = $env:parsed_issue_body

$json_object = @{ 
				Number = $issueNumber 
				Title = $issueTitle
				Description = $issueDescription
			} 

$json_string = ConvertTo-Json -Compress $json_object

$json_string = $json_string -replace '"', '\"'

Write-Output $json_string

