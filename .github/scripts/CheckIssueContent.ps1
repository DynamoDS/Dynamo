#Params
param([string]$issueContent="")

#Processing
function Check-For-Missing-Information([string]$issueContent){
    #Compares the issue content against the given pattern. 
    #(The '*' represent any amount of any characters)
    #True: if the content has 2 instances of "(Fill in here)"
    #False: if the pattern is not matched
    return $issueContent -like '*(Fill in here)*(Fill in here)*'
}

#Output
Check-For-Missing-Information $issueContent