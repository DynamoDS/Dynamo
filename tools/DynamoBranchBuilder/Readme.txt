Dynamo Branch Builder (DBB) Instructions:

The Dynamo Branch Builder utility allows you to build a branch of Dynamo, create the installers, and copy the resulting installer to a location that you specify.

Prerequisites:

 - Install InnoSetup. This is required to build the installers.
 - Install Github for Windows. This is required to call git.
 - Install Visual Studio. This is required for msBuild.

Local Setup:

 - DBB assumes that your github repositories are stored in <User>\Documents\Github. This should be the case if you've used the defaults in Github for Windows.
 - DBB assumes that git.exe is located in <User>\AppData\Local\GitHub\PORTAB~1\bin\git.exe. This should be the case if git is installed as part of Github for Windows.

Running from the command line:

DynamoBranchBuilder.bat <branch name> <output directory>

Output:

The output directory will contain an installer that looks like InstallDynamo_<branch name>.exe

