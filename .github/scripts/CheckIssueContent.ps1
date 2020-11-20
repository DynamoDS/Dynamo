#Params
param([string]$issueContent="")

#Processing
function Check-For-Missing-Information([string]$issueContent){
    return $issueContent -like '*(Fill in here)*(Fill in here)*'
}

#Output
Check-For-Missing-Information $issueContent