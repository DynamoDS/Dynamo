# AssemblyRenamer Tool

This tool uses mono.cecil to rename assemblies, the types they contain, and embedded resource types.
It's not thoroughly tested- always test the assemblies it generates carefully.

# Purpose
We are using it as an experiment in embedding popular binaries in order to avoid dll hell in contexts like Revit where Dynamo is just a single plugin among many others.

This tool's csproj is not built by default as part of any Dynamo.sln.

This tool's exe is not copied into Dynamo's main bin folder, it will be built locally under `src/tools/assemblyrenamer/bin/....`

## how to use

### note - whitepace included for easy reading - remove new lines in actual use.
```
 .\AssemblyRenamerCLI.exe -i ..\..\originalBinaries\Microsoft.Xaml.Behaviors.dll 
 -o C:\Users\[username]\Documents\Dynamo\extern\Microsoft.Xaml.Behaviors 
 -t Microsoft 
 -r Dynamo.Microsoft
```

```
  Required option 'i, InputAssembly' is missing.
  Required option 'o, OutputPath' is missing.
  Required option 't, TextToReplace' is missing.
  Required option 'r, ReplacementText' is missing.

  -i, --InputAssembly      Required. The assembly to perorm operations on.

  -o, --OutputPath         Required. Output path where generated assembly will be written to.

  -t, --TextToReplace      Required. A | sepereated list of string values to search of which will be replaced by strings
                           set by -ReplacementText.

  -r, --ReplacementText    Required. A | sepereated list of string values used as replacement text - length after
                           parsing must match the length of -TextToReplace.

  --help                   Display this help screen.

  --version                Display version information.
  ```