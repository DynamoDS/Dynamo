#Contains a collection of {Label, Keywords}. 
#"label" is the label to be use if specific "keywords" are found on a section
$labelsData = .\.github\LabelsKeywordsConfig.ps1

$label = 'undefined'
$issueTitle = $env:ISSUE_TITLE_PARSED

#Iterates over the posible labels
foreach ($labelData in $labelsData) {
    #Iterates over the different keywords for the specific label
    foreach ($keyword in $labelData.Keywords) {
        if ($issueTitle -match $keyword) {
            $label = $labelData.Label
        }
    }
    if($label -ne 'undefined') { break; }
}

#Response
if($label -ne 'undefined') 
    { Write-Output ($label)}
else 
    { Write-Output ('Valid') }
