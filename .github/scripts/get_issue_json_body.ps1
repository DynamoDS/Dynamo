param([int]$issueNumber)

$issueTitle = $env:ISSUE_TITLE
$issueDescription = $env:parsed_issue_body

$json_object = @{ 
				Number = $issueNumber 
				Title = $issueTitle
				Description = $issueDescription
			} 

$json_string = ConvertTo-Json -Compress $json_object

Write-Output $json_string

