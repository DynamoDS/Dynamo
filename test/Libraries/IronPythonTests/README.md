## IronPython Tests ##

This project has been disabled from the main Dynamo solution because Iron Python is no longer distributed with Dynamo and is no longer maintained.
In case Iron Python needs to be re-enabled follow the listed steps:
1. Re-enable the project to build within the solution - just add back the `.Build.0`  configurations in the Dynamo.sln (do the same thing for DSIronPython and IronPythonExtension)
2. Run the IronPython tests to make sure they pass
3. Modify the dll list at https://git.autodesk.com/Dynamo/DynamoBuildscripts/blob/master/eoJenkinsfile#L103