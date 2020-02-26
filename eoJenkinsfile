stage ('Starting Dynamo build job') {
    def branchToBuild = env.BRANCH_NAME
    build job: '../DynamoBuildscripts/master', parameters: [[$class: 'StringParameterValue', name: 'BranchDynamo', value: branchToBuild ]]
}
