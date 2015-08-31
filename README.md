![Image](https://ci.appveyor.com/api/projects/status/github/DynamoDS/Dynamo?branch=master) ![Image](https://travis-ci.org/DynamoDS/Dynamo.svg?branch=master)

![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png)
Dynamo is a visual programming tool that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages.


## Get Dynamo ##

Looking to learn or download Dynamo?  Check out [dynamobim.org](http://dynamobim.org/learn/)!


## Develop ###
### Create a Node Library for Dynamo ###
#### COMING SOON - NUGET PACKAGES ARE BEING BUILT AGAINST DYNAMO 0.8.2 ####
If you're interested in developing a Node library for Dynamo, the easiest place to start is by browsing the [DynamoSamples](https://github.com/DynamoDS/DynamoSamples).  
These samples use the [Dynamo NuGet packages](https://www.nuget.org/packages?q=DynamoVisualProgramming) which can be installed using the NuGet package manager in Visual Studio.

You can learn more about developing libraries for Dynamo on the [Dynamo wiki](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development).

### Build Dynamo from Source ###
You will need the following to build Dynamo:

- Microsoft Visual Studio 2013
- [GitHub for Windows](https://windows.github.com/)
- [Microsoft .NET Framework 3.5 with SP1](http://www.microsoft.com/en-sg/download/details.aspx?id=25150)
- Microsoft .NET Framework 4.0 and above (included with Visual Studio 2013)
- Microsoft DirectX (install from %GitHub%\Dynamo\tools\install\Extra\DirectX\DXSETUP.exe)

Directions for building Dynamo on other platforms (e.g. Linux or OS X) can be found [here](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-on-Linux,-Mac).  

Find more about how to build Dynamo at our [wiki](https://github.com/DynamoDS/Dynamo/wiki).


## Contribute ##

Dynamo is an open-source project and would be nothing without its community.  You can make suggestions or track and submit bugs via [Github issues](https://github.com/DynamoDS/Dynamo/issues).  You can submit your own code to the Dynamo project via a Github [pull request](https://help.github.com/articles/using-pull-requests).


## Releases ##

### 0.8.2 ###

Local Network Package and Definition location 
- use a common shared network folder within an office environment.

Preset Graph States
- Keep track of different states of your graph and develop design options with Presets, found in the Edit menu

Color on surfaces
-	Square arrays of color can now be applied to surfaces with the Display.BySurfaceColors node

Node2Code
-	Select groups of nodes and automatically create Code Block Nodes from the selection
-	Known issues that we will continue to work on

Search/Browse
-	Tooltips and search algorithm improvements for in canvas search (via right click)

Localization 
-	fit and finish

Documentation
- 4 New Chapters for the [Dynamo Primer](http://dynamobim.com/learn/)
- New and updated [Nuget packages for developers](https://www.nuget.org/packages?q=DynamoVisualProgramming)

Command Line Interface
-	DynamoCLI now available for executing non-Revit Dynamo graphs

Package Manager
- Publish a package locally from Dynamo for Revit

Known Issues
- Current list of [known issues](https://github.com/DynamoDS/Dynamo/wiki/Known-Issues)


### 0.8.1 ###

New Graphics pipeline
- Modernized geometry visualization capabilities to take more advantage of graphics hardware.
- Color: Dynmo now can represent colored geometry.  Check out the Display.ByGeometryColor capabilities. 
- See http://dynamoprimer.com/04_The-Building-Blocks-of-Programs/4-5_color.html
- Known issues for graphics hardware https://github.com/DynamoDS/Dynamo/wiki/Known-Issues

Graph Management
- Add Groups to your graph organization from the right click menu
- See http://dynamoprimer.com/03_Anatomy-of-a-Dynamo-Definition/3-4_best_practices.html 

Excel
- Improved handling of Excel.Read, including more robust management of null values and ragged lists
- Excel.Read now has a ReadFromFileAsString toggle, to preserve text imputs if desired
- Excel.Write now has the option to completely overwrite data in a sheet, or only the affected cells
- Excel.Write ignores popup messages 

Lists
- List.Transpose now keeps indices of lists consistent
- List.Clean removes null and empty lists from a given list, with or without preserving indices
- IF nodes will now lace over test input. Example, list with inputs {true, false, true}, {1,2,3}, {a,b,c} will result in {1,b,3}.  Previously, result would be {{1,2,3},{a,b,c},{1,2,3}}.  See submission https://github.com/DynamoDS/Dynamo/pull/4464

Revit
- FamilyInstance.SetRotation node

Localization
- Dynamo for Revit is now localized based on the Operating System locale.  

UI
- Control the preview state of multiple nodes at once in right-click menu
- Node port tooltips now show default inputs
- In canvas search available via Shift-DoubleClick and Right-Click
- Drag and drop nodes from the browser
- ExportToSAT now has units control
- Backup files are now created to recover lost work.  Backup folder location available in the Start page

Known Issues
- Current list of [known issues](https://github.com/DynamoDS/Dynamo/wiki/Known-Issues)


### 0.8.0 ###

#### New Features
User interface:
- More visually understandable and scannable node functionality with icons in the node library
- Expanded tooltip information in the node browser
- Improved keyword search capabilities

Custom Nodes:
- Lacing for Custom Nodes
- Default Values for Custom Nodes
- Type input tooltips for Custom Nodes

Localization:
- Unicode (Special Character) handling in Code Block Nodes and Data exchanged with other applications (like getting and setting Revit parameters)

Geometry
- Fillet and Chamfer for Solids and Polysurfaces
- New Mesh tools available on the Package Manager from MeshToolkit

Units
- Overhaul of the existing 0.7 Units handling for more legible interactions.  Details here:  http://dynamobim.com/units-in-dynamo-0-8-2/

Run Auto
- default state for new documents
- Run state is now saved per file (rather that set per session)

Development
- Revit libraries have been seperated out and now live in their own repository: https://github.com/DynamoDS/DynamoRevit
- Refactoring to provide a strong separation between what a Dynamo graph is and how it is displayed. This makes it easier for users to write powerful nodes, and for us to move the Dynamo platform forwards. https://github.com/DynamoDS/Dynamo/pull/3449

#### Fixes:
Namespace Collisions:
- Existing Code Block Nodes no longer affected by name collisions with functions that come from installed packages. For instance, Point.ByCoordinates in a Code Block Node was affected by a collision with a Point. operation in the popular Rhynamo package and would throw an error saying “Warning: Dereferencing a non-pointer. Dereferencing a non-pointer.”

Hardware Acceleration in Revit 2015
- Hardware Acceleration was turned off when running in Revit 2015.  Graphic speed and clarity is greatly improved

#### Notes:
- 0.8 is in a new folder structure to enable side by side installs with 0.7.  There is a one time only copy/paste of existing Packages from the 0.7 folder to 0.8 for your convenience

#### Known Issues
- Current list of [known issues](https://github.com/DynamoDS/Dynamo/wiki/Known-Issues)


###0.7.5 ###

#### New Features
- SAT files read from disk can be automatically updated in Dynamo graph using Geometry.ImportFromSAT
- Floor creation for structural types is now supported

#### Fixes
- Element.Geometry and Element.Face no longer crashes when used in Revit 2015 when executed on large groups of Revit geometry.
- Upgraded Excel.Write nodes no longer show as “Unresolved”.
- View.ExportAsImage will now export views other than default {3d}
- Dynamo does not conflict with other addins.  Previously, Dynamo would fail to launch in Revit when Unifi, Maxwell, Enum, or Kiwi Bonus Tools or a few other add-in were installed on Revit 2015.
- Users can now run Dynamo as an external program for debugging libraries in Visual Studio
- Better error messaging in Code Block Nodes
- modelcurve.bycurve no longer creates duplicate elements when adding to an array
- Copy/Paste of nodes now maintains lacing setting
- Many more bug fixes

#### Known Issues
- Current list of [known issues](https://github.com/DynamoDS/Dynamo/wiki/Known-Issues)

###0.7.4 ###

#### New features
- Automatic update of Dynamo from changes to files on disk.  Use File.FromPath nodes to drive changes from external files like Excel, images, and text files.  Files being read from disk are not locked, so you can edit them on the fly.
- Added hooks to allow for Dynamo for Structural Analysis (additional Package) workflows with Autodesk Robot.
- LoopWhile node for iterative workflows
- Package Manager sync and display improvements
- Easier to use Structural Framing nodes
- List.UniqueItems now works on Revit elements, strings, numbers, and geometry and also handles null values.
- Migration tools for 3rd party Library loading
- View selection via a dropdown

#### Bug fixes
- Improvements to Autocomplete (Autocomplete is off for comment areas, better handling of conflicts with 3rd party library class names, commit for autocomplete only with tab, enter, dot and single left clicking)
- Crash fixes for Package Manager
- Libraries loaded from disk or packages now only exposed needed nodes
- Changing Lacing triggers re-execcution of the graph
- Consistent notation for booleans (true/false)
- Import instance does not create multiple instances when regenerated
- Code Block Node output port positioning improvements
- GroupByKeys, List.Map, List.Scan fixes
- Surface.byLoft and Solid.byLoft fixes
- Error message improvements
- Changing location in revit is not picked up as a document change in Dynamo
- Custom node creation fixes (crash and bad input ports on creation)
- Curve extraction from surfaces now works on all surfaces
- Revit element creation and modification improvements, particularly around Views and Levels

###0.7.3 ###

#### New features
- Autocomplete in Code Block Nodes
- Share user-created binaries (.dll files) through the Package Manager
- Share sample content (.rvt, .rfa, .dyn files and more) through the Package Manager

#### Bug fixes
- Improvements to the core threading model (Scheduler)
- Reduced incidence of unresponsive graph
- Better error messages
- clearing and updating error messages
- Many small geometry improvements/fixes


###0.7.2 ###

####Incremental Release with:####
- Significant Stability Improvements
- More robust interaction with Revit elements
- Fixes to many small geometry tools
- Installer overhaul

####Some specifics####
- Preview Geometry in Revit (2015 Sundial Release only)
- Smaller download
- Directly open Samples folder from Help menu
- Object type is properly labeled on nodes for geometry outputs
- Improved documentation for 3rd party developers
- Advanced Tutorial Content
- Vast Regression Testing overhaul
- Solid.DifferenceAll(take one solid and get the Boolean difference with it versus a list of other solids0
- Copy Lacing and Nickname settings when copy/pasting
- List.UniquItems now works for geometry elements
- Improved handling of large numbers of geometric elements
- Improved Align Selection
- Improved converting Revit Walls to Dynamo geometry
- Improvements to search speed
- Improved handling of updates to 0.6.3 packages
- Improvements to If node uses in custom nodes
- Fixed Vasari-specific compatibility issues
- Fixed Model Line creations from Dynamo curves
- Fixed Surface.ByPatch error with closed polycurve
- Fixed Solid.ThinShell
- Fixed crashes in Revit document switching
- Fixed Mesh improperly scaled when extracted from Topography
- Fixed Curve.TangentAtParameter on curve from offset Polycurve crashes Dynamo
- Fixed crash using PolyCurve.Offset
- Fixes to Curve.Project behavior
- Fixes to Curve.PullOntoPlane behavior
- Fixed StructuralFraming.Location
- Fixed CBN not being parsed as a culturally invariant string
- Fixed Background preview level of detail for curves
- Fixed Copy/Paste of Code Block node problems
- Fixed nested List.Map
- Fixed Lacing issues on many nodes
- Fixed for Integer/Double interactions
- Fixed negative values in range expressions
- Fixes to multi-output nodes (Raybounce node and others)
- For more fixes, see https://github.com/DynamoDS/Dynamo/issues?q=is%3Aclosed+is%3Aissue+sort%3Aupdated-desc

####Back office Improvements:####
- Installer can now run silently for custom deployments
- MVVM refactoring: Standard Code separation and formatting for greater legibility and code reusability
- Separation from Revit dependencies for easier porting of Dynamo to new applications
- Recursion:  ScopeIf node for use in recursive custom node workflows (experimental)


###Older Releases###
[Archive of Release Fixes and Improvements](https://github.com/DynamoDS/Dynamo/wiki/Archive-of-Release-Fixes-and-Improvements)

## Instrumentation ##
Dynamo contains an instrumentation system that anonymously reports usage data to the Dynamo team. This data will be used to enhance the usability of the product. Aggregated summaries of the data will be shared back with the Dynamo community.

An example of the data communicated is:

"DateTime: 2013-08-22 19:17:21, AppIdent: Dynamo, Tag: Heartbeat-Uptime-s, Data: MTMxMjQxLjY3MzAyMDg=, Priority: Info, SessionID: 3fd39f21-1c3f-4cf3-8cdd-f46ca5dde636, UserID: 2ac95f29-a912-49a8-8fb5-e2d287683d94"

The Data is Base64 encoded. For example, the data field above ('MTMxMjQxLjY3MzAyMDg=') decodes to: '131241.6730208' This represents the number of seconds that the instance of Dynamo has been running.

The UserID is randomly generated when the application is first run. The SessionID is randomly generated each time Dynamo is opened.

## License ##

Dynamo is licensed under the Apache License. Dynamo also uses a number of third party libraries, some with different licenses. You can find more information [here](LICENSE.txt).

