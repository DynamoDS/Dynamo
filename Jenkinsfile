stage ('UI Test') {
    def averageBuildTime = 7200 // '../DynamoBuildscripts/master' average build time in seconds
    build job: '../DynamoAGTTests/master', propagate: false, quietPeriod: averageBuildTime
}
