#--Params--
#issueContent: Body of the issue to be analyzed
param([string]$issueContent)

#Loads requierements
. .\.github\scripts\issue_parser.ps1
#Contains a collection of {Label, Keywords}. 
#"label" is the label to be use if specific "keywords" are found on a section
$labelsData = .\.github\LabelsKeywordsConfig.ps1

#--Processing--
#Parse the issue
$parsed_issue_content = Get_Parsed_Issue $issueContent

$labels = @()
#Iterates over the posible labels
foreach ($labelData in $labelsData) {
    $keywordFound = $false
    #Iterates over the issue sections
    foreach ($section in $parsed_issue_content) {
        #Iterates over the posible keywords for each label
        foreach ($keyword in $labelData.Keywords) {
            if(($section.Title -match $keyword) -or ($section.Content -match $keyword)){
                $keywordFound = $true
            }
        }
    }
    if($keywordFound){
        $label = $labelData.Label
        $labels = $labels + "$label"
    }
}

#Response
if($labels.count -gt 0){
    #ouput: label names as strings separated by commas
    #example: "label1", "label2"
    Write-Output ('"' + ($labels -join '", "') + '"')
}
else{
    Write-Output ("Valid")
}

