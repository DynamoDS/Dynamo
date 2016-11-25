This folder contains scripts to build and post the DynamoVisualProgramming NuGet packages into NuGet Gallery and Artifactory.

To build NuGet packages locally, run BuildPackages.bat.

To post them as pre-release packages into NuGet Gallery and Artifactory, skip the above step and run BuildAndPostPackages.bat.
Supply the NuGet Gallery API Key as the first parameter and the Artifactory API Key as the second parameter for this batch file.