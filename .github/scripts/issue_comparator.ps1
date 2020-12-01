<#
.SYNOPSIS
Compare two arrays with template/issue information

.DESCRIPTION
 This method will receive two parameters of type Array[Hashtable], the first one contains the template information (ISSUE_TEMPLATE.MD) the second one contains information about the issue filled in github/issues section.

.PARAMETER InputTemplateArray
Array[Hashtable] containing the template information (ISSUE_TEMPLATE.MD)

.PARAMETER InputIssueFilledArray
Array[Hashtable] containing the issue information filled by the user in github/issues section
#>
function Compare_Issue_Template($InputTemplateArray, $InputIssueFilledArray)
{
	$ComparisonArrayResult = @()

	$TitleStatus = ""
	$ContentStatus = ""
	
	$InputTemplateArray | ForEach-Object { 
		
		#Write-Output $_.Title
		$TemplateEntry = $PSItem
		$IssueContent = ""
		ForEach($entry in $InputIssueFilledArray ) { 
			$IssueEntry = $entry
			$EntryTitle = $IssueEntry.Title.Trim()
			$TemplateTitle = $TemplateEntry.Title.Trim()
			$TemplateContent = $TemplateEntry.Content.Trim()
			$IssueContent = $IssueEntry.Content
			If ($EntryTitle -Contains $TemplateTitle)
			{						
				#Means that the title is the same than in the template, so it should be empty		
				if ($EntryTitle -eq $TemplateTitle)
				{
					$TitleStatus = "Equal"
					If (($IssueContent -eq $TemplateContent) -or ([string]::IsNullOrEmpty($IssueContent)))
					{
						#The template content is the same than the issue content or the content is empty
						$ContentStatus = "Empty"
					}
					else
					{
						#The issue content is different than the template then it was filled by the user
						$ContentStatus = "Filled"
					}	
					$IssueContent = $IssueContent				
					break
				}
				else 
				{
					#The title doesn't match the template then was updated (even when the case was changed)
					$TitleStatus = "Updated"
				}	
			}
			else
			{
				#The title was not found the was deleted by the issue creator
				$TitleStatus = "NotFound"
				$ContentStatus = "NotFound"
			}		
		}

		#Add the resulting values to the array
		$ComparisonArrayResult += (@{ 
			Title=$TemplateEntry.Title;
			Content=$IssueContent;
			TitleStatus=$TitleStatus;
			ContentStatus=$ContentStatus;
		})
	}

	Write-Output $ComparisonArrayResult -NoEnumerate
}