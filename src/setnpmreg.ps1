If ((Test-Connection git.autodesk.com).PingSucceeded){
    echo host reachable
    npm set registry https://npm.autodesk.com/artifactory/api/npm/autodesk-npm-virtual/
}
else{
    echo host not reachable
    npm set registry https://registry.npmjs.org
}
