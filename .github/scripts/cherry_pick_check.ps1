param([string]$commitMessage)

#Looks for the cherry-pick command on the commit message
#Format: Cherry-pick to 'target-branch'
$cherrypickCommand = $commitMessage | Select-String -Pattern "Cherry-pick to\s?'*'"

#Extracts the branch name
if($cherrypickCommand){
    $splitedData = $cherrypickCommand.Line.split("'")
    $branchName = $splitedData[1]
    if($branchName){
        Write-Output $branchName
        exit
    }
}
Write-Output 'invalid'