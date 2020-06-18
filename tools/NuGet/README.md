# About

This folder contains scripts to build and publish the DynamoVisualProgramming NuGet packages into NuGet Gallery and Autodesk Artifactory.

## Instructions

To build NuGet packages locally, run BuildPackages.bat with parameters.

```bash
.\BuildPackages.bat param1 param2(optional)
```

*param1* should be the path where the nuspec files exist

*param2* should be the path where the dynamo binaries exist(optional, if it is not provided, it falls back to the `harvest` folder assuming dev has run the Install.sln)

See Example:

```bash
.\BuildPackages.bat "template-artifactory" "...\GitHub\Dynamo\src\DynamoInstall\harvest"
```

> Note: To build Install.sln solution you will need wix installed.

This command will wrap the dlls in the `harvest` folder into `nupkg` files and put them in current folder.

Alternatively, you can provide the second parameter as the path to your Dynamo binaries built with Release configuration, such as:

```bash
.\BuildPackages.bat "template-artifactory" "...\GitHub\Dynamo\bin\AnyCPU\Release"
```

To post them as pre-release packages into NuGet Gallery and Artifactory, skip the above step and run PostNugetPackages.bat or PostArtifactoryPackages.bat.
Supply the NuGet Gallery API Key as the first parameter and the Artifactory API Key as the second parameter for this batch file.
