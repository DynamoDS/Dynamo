<#
.SYNOPSIS
Compare two arrays with template/issue information

.DESCRIPTION
 This method will receive two parameters of type Array[Hashtable], the first one contains the template information and the second one contains information about the issue filled in github/issues section.

.PARAMETER InputTemplateArray
Array[Hashtable] containing the template information

.PARAMETER InputIssueFilledArray
Array[Hashtable] containing the issue information filled by the user in github/issues section
#>
function Compare_Issue_Template($InputTemplateArray, $InputIssueFilledArray) {

	$ComparisonArrayResult = @()

	foreach ($templateSection in $InputTemplateArray ) { 
		
		$TemplateTitle = $templateSection.Title.Trim()
		$TemplateContent = $templateSection.Content.Trim()
		
		$IssueSectionContent = ""
		$ContentStatus = ""
		$TitleStatus = ""

		foreach ($issueSection in $InputIssueFilledArray ) { 

			$IssueSectionTitle = $issueSection.Title.Trim()
			$IssueSectionContent = $issueSection.Content
					
			#Means that the title is the same than in the template, so it should be empty		
			if ($IssueSectionTitle -eq $TemplateTitle) {
				$TitleStatus = "Found"
				
				If (($IssueSectionContent -eq $TemplateContent) -or ([string]::IsNullOrEmpty($IssueSectionContent))) {
					#The template content is the same than the issue content or the content is empty
					$ContentStatus = "Empty"
				}
				else {
					#The issue content is different than the template then it was filled by the user
					$ContentStatus = "Filled"
				}	
				break
			}	
		}

		if($TitleStatus -eq ""){
			#The title was not found, then it was deleted by the issue creator
			$TitleStatus = "NotFound"
			$ContentStatus = "NotFound"
			$IssueSectionContent = ""
		}

		#Add the resulting values to the array
		$ComparisonArrayResult += (@{ 
				Title         = $templateSection.Title;
				TitleStatus   = $TitleStatus;
				Content       = $IssueSectionContent;
				ContentStatus = $ContentStatus;
			})
	}

	#Checks and adds the information about new sections on the issue to the response array
	foreach ($issueSection in $InputIssueFilledArray ) { 

		$IssueSectionTitle = $issueSection.Title.Trim()
		$IssueSectionContent = $issueSection.Content
		$SectionFound = "False"
				
		foreach ($foundSection in $ComparisonArrayResult ) { 
			#If the sections was already included in the response array		
			if ($IssueSectionTitle -eq $foundSection.Title.Trim()) {
				$SectionFound = "True"
			}	
		}

		if($SectionFound -eq "False"){
			$ComparisonArrayResult += (@{ 
				Title         = $IssueSectionTitle;
				TitleStatus   = "New";
				Content       = $IssueSectionContent;
				ContentStatus = $ContentStatus;
			})
		}	
	}
	$ComparisonArrayResult
}
