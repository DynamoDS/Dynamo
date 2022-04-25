
<#
.SYNOPSIS
#Parse the issue/template passed as parameter and returns an Array[Hashtable]

.DESCRIPTION
This method parse the issue template located in github repository (ISSUE_TEMPLATE.MD) and returns an Array[Hashtable] structure with all the information (Title/Content/TitleStatus/ContentStatus)
Receives as a parameter the template defined in the github repo or the template filled by the user with the issue information

.PARAMETER InputTemplateString
Parameter issue filled or template (using github Markdown)
e.g.
## Dynamo version
5.6.1.1
## Operating system
Windows 10
## What did you do? 
Just testing
## What did you expect to see?
Any result
## What did you see instead?
Nothing else
#>
function Get_Parsed_Issue ($InputTemplateString)
{
	[System.Collections.ArrayList]$ParsedTextArray = @()

	#All the titles/content in the github template should start with the characters ##, then this regex will get title
	$TitlesArray = $InputTemplateString | Select-String -Pattern "##.*\n" -AllMatches

	#Once having the titles iterates over the all text in order to get the content (the text between the first title ##  and the second title ##)
	for ($i = 0; $i -lt $TitlesArray.Matches.Count; $i++) {
		$StartIndex = $TitlesArray.Matches[$i].Index + $TitlesArray.Matches[$i].Value.Length
		if (($i+1) -lt $TitlesArray.Matches.Count){
			$EndIndex = $TitlesArray.Matches[$i+1].Index
		}
		else {#Means that we reach the end of the text when we need to get the text from the last title ## and the last character
			$EndIndex = $InputTemplateString.Length
		}	
		#Remove the characters \n \r y # from the Title
		$TitleValue = $TitlesArray.Matches[$i].Value -replace "`n","" -replace "`r","" -replace "#"

		#Remove the characters \n \r from the Content
		$ContentValue = $InputTemplateString.Substring($StartIndex, $EndIndex - $StartIndex) -replace "`n","" -replace "`r",""

		#Add returns an index when we need to catch it in a variable otherwise will be returned
		$null = $ParsedTextArray.Add(@{Title=$TitleValue; Content=$ContentValue})
	}
	Write-Output $ParsedTextArray -NoEnumerate
}