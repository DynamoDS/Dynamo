#--Params--
#issueTemplateFile: Name of the template file including extension (ex: ISSUE_TEMPLATE.md)
#issueContent: Body of the issue to be analyzed
#acceptableEmptyFields: Amount of fields from the template that can be missing information
#                       in the issue (1 if unspecified)
param([string]$issueTemplateFile, [int]$acceptableEmptyFields = 1)

#Loads the requiered functions
. .\.github\scripts\issue_comparator.ps1
. .\.github\scripts\issue_parser.ps1

#--Processing--
$issueTemplate = Get-Content -Raw -Path .github\$issueTemplateFile
$issueContent = $env:ISSUE_BODY

#Parse the template and issue
$parsed_issue_content = Get_Parsed_Issue $issueContent
$parsed_issue_template = Get_Parsed_Issue $issueTemplate

#Compares the tempalte and issue
$comparation_result = Compare_Issue_Template $parsed_issue_template $parsed_issue_content

$analysis_result = " "
[int]$missingFields = 0

#Checks for missing content on the comparator result and loads
#$analysis_result with the corresponding section title
$FullyEmpty = "True"
foreach ($Section in $comparation_result) {

    if ($Section.TitleStatus -ne "New") {
        if (($Section.ContentStatus -eq "Empty") -or ($Section.ContentStatus -eq "NotFound")) {
            $script:analysis_result = "$($script:analysis_result) \n- $($Section.Title)"
            $script:missingFields = $script:missingFields + 1
        }
        else {
            $FullyEmpty = "False"
        }
    }
}

if($FullyEmpty -eq "True") { $analysis_result = "Empty" }

#If no missing information was found then the issue is Valid
if (($analysis_result -eq " ") -or ($missingFields -le $acceptableEmptyFields)) { $analysis_result = "Valid" }

#--Output--
#"Valid" if the issue has the necessary information
# or string with section titles if information is missing
$analysis_result