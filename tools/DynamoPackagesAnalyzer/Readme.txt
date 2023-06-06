This tool requires analyzebinaries experimental feature of upgrade-assistant enabled
https://github.com/dotnet/upgrade-assistant#experimental-features

This tool has three operation modes:

1. zipfile
This mode expects an array of zip archives to be analyzed one by one

Flags explanation:
--files: used to receive the files to process
-l: when this flag is present, the packages list is downloaded from dynamopacakges.com, and using the name property from pkg.json,
    the process tries to resolve the complete package information matching the name with the packages from the downloaded list

Example:
DynamoPackagesAnalyzer.exe zipfile --files "some\path\package1.zip" "some\path\package2.zip" -l

/////////////////////////////////////////////////////////////////////////////////////////////////////

2. directory
This mode expects a path where the package zip archives are located, to be analyzed one by one

Flags Explanation:
-l: when this flag is present, the packages list is downloaded from dynamopacakges.com, and using the name property from pkg.json,
    the process tries to resolve the complete package information matching the name with the packages from the downloaded list

Example:
DynamoPackagesAnalyzer.exe directory "some\path\" -l

/////////////////////////////////////////////////////////////////////////////////////////////////////

3. dynamopackages
This mode downloads and analyzes all the packages at dynamopackages.com

Flags Explanation:
No flags required

Example:
DynamoPackagesAnalyzer.exe dynamopackages

/////////////////////////////////////////////////////////////////////////////////////////////////////

For all modes:
The results files can be found at %temp%/DynamoDS or at the workspace path defined in appsettings.json

1. results[yyyy-MM-dd_HH-mm-ss].csv: contains the analysis for al the packages at dynamopackages.com
2. duplicated[yyyy-MM-dd_HH-mm-ss].csv: contains a list of dlls names that were analyzed two or more times in distinct packages

The properties Name and Result in results[yyyy-MM-dd_HH-mm-ss].csv file, replaces the character ',' by '&'

/////////////////////////////////////////////////////////////////////////////////////////////////////

appsettings.json:
MaxDegreeOfParallelism: (default: 4) Defines the number of threads to analyze the packages list in any mode

DynamoPackagesURL: The URL to list and download packages from

SarifFileName: The upgrade-assistant result file name

Workspace: (default: %temp%\DynamoDS) when null, the default value is used, it can be any directory with read and write permissions, this workspace is used to download
 the packages and unzip them as part pof the analysis.

ProcessTimeOut: (default: 2) Defines the time to wait in minutes for a upgrade-assistant instace to end in case of the instace not being able to finish by itself.
