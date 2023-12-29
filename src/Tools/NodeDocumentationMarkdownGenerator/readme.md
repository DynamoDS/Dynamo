## Node Documentation Generator

The NodeDocumentationGenerator is a CLI tool to generate node documentation markdown stubs. The tool creates markdown files.
Depending on the mode and option flags used, the file content can be default content, or extracted from the DynamoDictionary.

There are three different high level commands:
1. Create Documentation From Directory
2. Create Documentation From Package
3. Rename an existing Documentaion package to a shorter name

## Use Cases

1. One time dynamo dictionary migration to .md files. The resulting files will be stored in the Dynamo and host specific repos. (DynamoRevit)

2. Continual creation of new docs stubs for use on dictionary and in Dynamo docs browser. DynamoDictionary site will consume .md files.

3. Used by package authors to stub out documentation for nodes.
third parties can run the --FromPackage command to stub out empty markdown files in the /doc folder of their package. Then the package author can manually fill in the markddown file with detailed documentation, images, gifs etc.

4. The name/path of a resulting documentaion package sometimes gets too long and causes problems for CI/CD and installers. The rename options can then be used on a case by case basis for shorten the base filename to a name based on a hash value. The MD file and all support files gets renamed and the original base name is added to the new MD file as a comment making it searchable.

## How to use the Generated Docs

### Dynamo
Dynamo loads documentation markdown files using documentationbrowser view extension. It loads documentation from packages, from the host_fallback_docs folder, and finally from the core fallback_docs folder.

The documentation for core nodes that are not imported from packages should be generated with this tool, or manually created, then added to the fallback docs folders, then the docs browser will be able to display them when users request help on specific nodes.

### Dictionary
Eventually Dictionary website can be refactored to consume markdown files from core and each host, that way the documenation between core and dictionary is always in sync.


## Using the Tool with CLI options

### Verbs
| Verb            | Description                                                                                                                                                                                                 |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `fromdirectory` | Generates documentation for all dlls (or only those specified in the filter) and all dyfs (if specified by the  `--includedyfs` flag) in the input directory                                |
| `frompackage`   | Looks up node libraries in the pkg.json file and creates documentation for those dlls and creates documentation for all dyfs in the `/dyf` folder. All generated files gets saved in `path/to/package/docs` |
| `rename`        | Renaming utilities for fallback MD files |

### fromdirectory - flags
| Short name | Long name           | Optional | Description                                                                            |
| ---------- | ------------------- | :------: | -------------------------------------------------------------------------------------- |
| `-i`       | `--input`           |          | Input binary file, containing nodes that documentaion should be generated for.         |
| `-o`       | `--output`          |          | Location where markdown files should be generated                                      |
| `-r`       | `--references`      |    ✅     | optional flag - Folder paths to dlls that are used as references in the nodes          |
| `-f`       | `--filter`          |    ✅     | optional flag - Specifies which binary files documentation should be generated for     |
| `-c`       | `--includedyfs`     |    ✅     | optional flag - Include custom dyf nodes                                               |
| `-d`       | `--dictionary`      |    ✅     | optional flag - File path to DynamoDictionary json                                     |
| `-w`       | `--overwrite`       |    ✅     | optional flag - When specified the tool will overwrite files in the output path        |
| `-y`       | `--recursive-scan`  |    ✅     | optional flag - Input folder will be scanned recursively                               |
| `-s`       | `--compress-images` |    ✅     | optional flag - When set, the tool will try to compress images from dictionary content |
| `-g`       | `--compress-gifs`   |    ✅     | optional flag - When set, the tool will try to compress gifs from dictionary content   |
| `-x`       | `--layout-spec`     |    ✅     | optional flag - Path to a LayoutSpecification json file                                |
| `-v`       | `--verbose`         |    ✅     | optional flag - print debug information                                                |

### FromDirectory examples

generate docs for CoreNodeModels.dll, importing dictionary content, and compressing images. For a list of OOTB Dynamo node binaries, refer to `DynamoPathResolver` class in Dynamo code base.

```bash
fromdirectory 
-i "C:\Users\....\Documents\GitHub\Dynamo\bin\AnyCPU\Debug\nodes" 
-f CoreNodeModels.dll 
-o "C:\Users\...\Desktop\TestMdOutput_CoreNodeModels" 
-d "C:\Users\...\Documents\GitHub\DynamoDictionary\public\data\Dynamo_Nodes_Documentation.json" 
-x "C:\Users\...\Documents\GitHub\Dynamo\src\LibraryViewExtension\web\library\layoutSpecs.json" 
-w -s
```
generate docs for DSRevitNodesUI.dll, importing dictionary content, and compressing images.

```bash
fromdirectory 
-i "C:\Users\...\Documents\GitHub\DynamoRevit\bin\AnyCPU\Release\Revit\nodes" 
-f DSRevitNodesUI.dll 
-o "C:\Users\...\Desktop\TestMdOutputRevitNodes - Compressed" 
-d "C:\Users\...\Documents\GitHub\DynamoDictionary\public\data\Dynamo_Nodes_Revit.json" 
-x "C:\Users\...\Documents\GitHub\DynamoRevit\src\DynamoRevit\Resources\LayoutSpecs.json" 
-r "C:\Users\...\Desktop\PathToFolderWithRevitAPI" "C:\...\DynamoRevit\bin\AnyCPU\Debug\Revit"
-w -s
```

### frompackage - flags
| Short name | Long name      | Optional | Description                                                                     |
| ---------- | -------------- | :------: | ------------------------------------------------------------------------------- |
| `-i`       | `--input`      |          | Package folder path.                                                            |
| `-r`       | `--references` |    ✅     | optional flag - Folder paths to dlls that are used as references in the nodes   |
| `-w`       | `--overwrite`  |    ✅     | optional flag - When specified the tool will overwrite files in the output path |
| `-v`       | `--verbose`         |    ✅     | optional flag - print debug information                                                |

### FromPackage examples

generate docs for a package which depends on the revitAPI.

```bash
frompackage 
-i "C:\Users\...\source\repos\MyRevitPackage\dist\packageFolder" 
-r "C:\Users\...\Program Files\Autodesk\Revit 2022"
-w
```

### rename - flags
| Short name | Long name      | Optional | Description                                                                     |
| ---------- | -------------- | :------: | ------------------------------------------------------------------------------- |
| `-f`       | `--file`       |    ✅     | Input MD file. Renames a single MD file including any support files to a shorter length (~56-60 characters) base file name.                                                            |
| `-d`       | `--directory`  |    ✅     | Input directory. Inspects all MD files in a directory and renames all MD files with a base name longer that maxlength (see below).   |
| `-m`       | `--maxlength`  |    ✅     | Max length of the base file name before renaming to a shorter length (~56-60 characters) base file name. Defaults to 70. |


### Rename examples

rename a single MD file including any support files

```bash
rename 
-f "C:\...\fallback_docs\Autodesk.DesignScript.Geometry.CoordinateSystem.ByOriginVectors(origin, xAxis, yAxis).md" 
```

rename all MD files in a directory with a base file name longer then 70 characters.

```bash
rename 
-d "C:\...\fallback_docs"
-m 70
```

## Known Issues:
* The nodedocsgenerator.exe tool currently requires being able to load the types used by the binaries being inspected. For example
if you are trying to generate docs for a package which depends on Revit you will need to use the `-references flag (-r)` to give the tool access to the RevitAPI and Revit binaries. 
    * There is a version of this tool that attempts to use `MetaDataLoadContext` to avoid this requirment, but it was deemed too complex at the current time, see: https://github.com/SHKnudsen/Dynamo/tree/Node-Markdown-generator-tool for this version.

* When creating stub markdown files for nodes without linkned dictionary content the default content is not very useful in a dictionary context. At runtime in Dynamo automatic content can be generated for inputs, outputs, and description, but in dictionary this content cannot be generated. Eventually this tool should generate this default content, and it should be filtered out if present in the Dynamo context.

* when using MetaDataContextLoad there are two issues at least that result in hundreds of nodes not correctly having docs files generated.
    * Because in metaDataContext mode the potential assembly load order is different from a normal dynamo session - some imported types that conflict with built-in geometry types maybe imported first. This causes other nodes to fail to be imported. You will see errors in the console about namespace support from protocore. Importing `Revit.Element.Area` nodes will reproduce this when imported with metaDataContext.
    * An issue with retrieving defaultvalue strings - reported to dotnet org here: https://github.com/dotnet/runtime/issues/57238 - causes any class that has a member that has a default value of `string.empty` to fail to import. `Revit.Dimension` nodes will reproduce this when imported with metaDataContext.
