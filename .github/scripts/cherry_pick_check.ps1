param([string]$commitMessage)

#Looks for the cherry-pick command on the commit message
$commitData = $commitMessage | Select-String -Pattern "Cherry-pick to:\s?'*'"

#Extracts the branch name
if($commitData){
    $splitedData = $commitData.Line.split("'")
    $branchName = $splitedData[1]
    if($branchName){
        Write-Output $branchName
        exit
    }
}
Write-Output 'invalid'