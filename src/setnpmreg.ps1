If ((Test-Connection git.autodesk.com).PingSucceeded){
    echo adsk reachable
    New-Item -Path . -Name ".npmrc" -ItemType "file" -Value "registry=https://npm.autodesk.com/artifactory/api/npm/autodesk-npm-virtual/" -Force
}
else{
    echo adsk not reachable
    New-Item -Path . -Name ".npmrc" -ItemType "file" -Value "registry=https://registry.npmjs.org" -Force
}
