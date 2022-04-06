param([int]$issueNumber, [string]$issueTitle, [string]$issueDescription)

$json_object = @{ 
				Number = $issueNumber 
				Title = $issueTitle
				Description = $issueDescription
			} 

$json_string = ConvertTo-Json -Compress $json_object

$json_string = $json_string -replace '"', '\"'

Write-Output $json_string

