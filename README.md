![Image](https://ci.appveyor.com/api/projects/status/github/DynamoDS/Dynamo?branch=master) ![Image](https://travis-ci.org/DynamoDS/Dynamo.svg?branch=master)

![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png)
Dynamo is a visual programming tool that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages.


## Get Dynamo ##

Looking to learn or download Dynamo?  Check out [dynamobim.org](http://dynamobim.org/learn/)!


## Develop ###
### Create a Node Library for Dynamo ###
If you're interested in developing a Node library for Dynamo, the easiest place to start is by browsing the [DynamoSamples](https://github.com/DynamoDS/DynamoSamples).  
These samples use the [Dynamo NuGet packages](https://www.nuget.org/packages?q=DynamoVisualProgramming) which can be installed using the NuGet package manager in Visual Studio.

[Documentation of the Dynamo API]( http://dynamods.github.io/DynamoAPI) with a searchable index of public API calls for core functionality. This will be expanded to include regular nodes and Revit functionality.

The [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes) document explains changes made to the Dynamo API with every version.

You can learn more about developing libraries for Dynamo on the [Dynamo wiki](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development).

### Build Dynamo from Source ###
You will need the following to build Dynamo:

- Microsoft Visual Studio 2015
- [GitHub for Windows](https://windows.github.com/)
- Microsoft .NET Framework 4.5.
- Microsoft DirectX (install from %GitHub%\Dynamo\tools\install\Extra\DirectX\DXSETUP.exe)

Directions for building Dynamo on other platforms (e.g. Linux or OS X) can be found [here](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-on-Linux,-Mac).  

Find more about how to build Dynamo at our [wiki](https://github.com/DynamoDS/Dynamo/wiki).


## Contribute ##

Dynamo is an open-source project and would be nothing without its community.  You can make suggestions or track and submit bugs via [Github issues](https://github.com/DynamoDS/Dynamo/issues).  You can submit your own code to the Dynamo project via a Github [pull request](https://help.github.com/articles/using-pull-requests).


## Releases ##

### 1.3.3 ###
New Functionality: 

- Compatibility with Revit 2019

Known Issues:
- Some keyboard shortcuts do not work in Dynamo for Revit 2019. These include Ctrl+Z for Undo and Ctrl+C/Ctrl+V for Copy/Paste.

### 1.3 ###
See http://dynamobim.org/dynamo-1-3-release/ for a full report

New Functionality

Core
- Geometry Working Range, a solution that adjusts numerical precision to accommodate these large numbers.
- Isolate Selected Geometry 
- A direct link to the Dynamo Dictionary is added at the bottom of help menu popup window for each node. Definitions are currently  available for the out-of-the-box core Dynamo nodes, and we are actively working on Revit nodes.
- Multi-output port re-connect feature using Shift+Left-Click
- Highlight geometry from selection in preview bubbles and watch nodes.
- DateTime.Format: (shout out to Radu Gidei!)

Dynamo Studio:
- Translate (CAD import) functionality now supports 3DM (Rhino) and SAT file formats, in addition to DWG and DXF.  (Additionally, import of FBX, OBJ, STL, and SKP formats are supported, but there is currently limited downstream capabilities with these meshes.)   

Dynamo for Revit:
- Revit allows for duplicate parameter names in a file, and Dynamo is now able to consistently pick between Named or Built-In parameters. 
- Preview Bubbles enabled for Revit Selection nodes
- New Revit nodes: a number of Creation methods for System Families, added access to Global Parameters, and exposed file auditing tools with the Performance Advisor. 

Bug Fixes
- Fixed line break issue in for group titles, increased group titles font size
- Convert between units now gives correct Hectares for Area conversion
- Fixed File Paths with spaces sometimes converting paths to escaped URI path
- Fixes to Arc.ByCenterPointRadiusAngle, Arc.StartAngle, and Arc.EndAngle


### 1.2.1 ###

Important Bug Fixes:
- Full Installation of Dynamo for Revit included in the Revit 2017.1 release will no longer remove Dynamo 1.x installations in previous versions of Revit.  Dynamo for Revit 1.2 will now also be installed for any installation of Revit 2015 or 2016 as well.
- Dynamo for Revit is now included in Revit 2017.1 network deployments.
- Dynamo Player no longer takes all of 1 processor when launched.

### 1.2.0 ###

New Functionality : 
- New out-of-the-box Revit functionality, including FaceBased Family creation, Coordinates, Detail Curves, Dimensions, Filled Region, Filters, Location enhancements, Materials, Parameters, Revisions, Rooms, Tag, Text notes, and Element queries.
- New list management tools found on node input ports that can greatly simplify many workflows and reduce the need for List.Map and List.Combine in most places.
- Geometry Preview state of nodes shown in color change

Important Bug Fixes:
- 4 most common crashes identified through crash error reporting have been fixed.  These failures were a combination of divide by zero and Null reference exception thrown from preview and some issues during initialization with Revit, which account for 30% of all reported crashes in Dynamo 1.1.
- Graphs with large vertex counts consume 1 processor when program is idle
- All Elements Of Category fails for Views
- Package Manager Crash and upload fixes
- Custom node can only place family instance(s) in isolation. Multiple use of same CN on graph results in 'last node wins'
- Number from Feet and Inches rounds input text incorrectly and output is confusing
- Circle.ByBestFitThroughPoints is always horizontal
- Preview Control Crash when deleting ObjectSelector node before running completed
- Crashes with adjacent install of some Autodesk products
- Solid By Sweep 2 Rails incorrect with rotated NURBS curve
- Opening dyn file whose dyn extension is missing corrupts Dynamo
- Arc.ByStartPointEndPointStartTangent doesn't work well with all Tangents
- Graphs with large vertex counts consume whole processor when program is idle
- CoordinateSystem Scale methods do not work
- Add option to Curve.ByBlendBetweenCurves to create G2 continuous blend curves

### 1.1.0 ###

New Functionality : 
- A whole new bunch of Library added for T-Spline Nodes. You can turn on this option from Settings to view the T-Spline nodes in Library. T-Splines modelling technology is now exposed in Dynamo to enable new organic and freeform geometry creation tools. There are approximately 150 new nodes that are a subset of the existing geometry library. Since this is an advanced functionality that may be useful only in certain non-standard workflows these nodes by default will be hidden in the library and will not interfere in node search results unless explicitly turned on in the Settings -> Experimental menu. The new functionality offers a wide range of capabilities to create and edit T-Spline surfaces and also conversion to and from NURBS and meshes.
- New notification center provides you with more details on system crashes and errors, such as when DLL incompatibilities between Dynamo and other Revit Addins are detected at startup
- New Settings menu option to Show or Hide Preview Bubbles
- We have fixed the long pending issue of Screen Capture, now it doesn't matter how big your graph is, everything will be visible  at whatever zoom level you are at when you use Export Workspace as Image.

Important Bug Fixes: 
- Element Binding (the ability of Dynamo to track and modify rather than duplicate or replace elements in Revit) had some regressions in Revit 2017.  These are fixed.
- Freeze does not delete elements created in Revit anymore.
- Fixed issue related to FamilyInstance.SetRotation, now you can use this node with Run Automatic mode as well and your first instance won't get placed randomly in a different location and crash issues have been addressed
- Fixed long pending issue with Importing series of Swept Solids, Now while after importing all Surfaces are there. 
- Localization crash fixes with Norwegian, German and French
- Dynamo for Revit no longer crashes at startup with non-compliant Views (not 3D)
- When selecting any labeled item only that item's label gets displayed, no additional labels are displayed
- Fixed errors on Code Block Nodes and String Nodes that are published to Web and accessed from the Customizer view
- Read-only nodes and directories can now be loaded in Dynamo.
- Mapping flatten nodes and flatten on single values no longer replaces data with null values
- Arc.ByStartPointEndPointStartTangent no longer fails using a normalised vector
- Direct Shape now recognizes material input properly
-  /verysilent install of Dynamo for Revit now completes without user interaction
-  Fixed crash with closed curve as input to Surface.ByLoft
-  Dot product no longer returning erroneus scalars
-  Zero radius and related bad geometry errors no longer create crash 
-  Turning off "Revit Background Preview" no longer turns off "Background Preview" on relaunch of Dynamo.
 
Other Changes: 
- Now once you add a new path for Package location then all the packages from the new path will get loaded without relaunching Dynamo.
- We have improved the preview bubble for its Pin and hover over related issues.
- Updated the compact view of the preview bubble to display information about the number of items in an output list.
- Quick Access to "Getting Started" from Help menu
- From this release, we stopped migrating of 0.6.3 and 0.7.0 files. If your old files contain nodes from above two releases then you have to open those files on an earlier version (till 1.0.0) and the save them. 

Known Issues:
- Installing Dynamo 4 Revit 1.1 Will require a reinstallation of Dynamo Studio with Studio 1.1 (1.0 and 1.1 cannot co-habitate).  This issue will not happen in the future, when Future Dynamo Core installation will work with older versions of Products. 
- Simplification of some overload methods will result in minor changes in behavior.  Please see this document for specific nodes affected: https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Node-Changes

### 1.0.0 ###

- API Stabilization:  1.0.0 is a commitment to stable code that allows for smoother and more reliable movements from one version to another.  To more clearly express this, we have been moving to “semantic versioning” to illustrate the nature of changes in each release. We will be using the fairly standard version naming with an x.y.z system, where x incrementing represents breaks to the API (requiring developer refactors), y indicates changes that are still backwards compatible, and z are smaller bug fixes.  Package creators and maintainers are encouraged to assess changes to the previous code, which can be found here  

  https://github.com/DynamoDS/Dynamo/wiki/Dynamo-Node-Changes  

  https://github.com/DynamoDS/Dynamo/wiki/API-Changes
- Graphics performance enhancements:  see this post for details  
  https://github.com/DynamoDS/Dynamo/pull/6356
- Documentation: Along with new sections of the DynamoPrimer (http://DynamoPrimer.com), we have started an online documentation of the Dynamo API with a searchable index of public API calls for core functionality. This will be expanded to include regular nodes and Revit functionality.  http://dynamods.github.io/DynamoAPI/
- Licensing:  Dynamo Studio is now using a new version of the Autodesk installer that allows for easier access to network and token flex licensing tools
- Install:  we have created a separate installation for "core" Dynamo functionality, those tools used by all implementations of Dynamo, and Revit, and Studio installations.  This allows for the sharing of a common core of Dynamo code and packages.
- List Management:  Changes to "replication" or automated matching of different data streams in nodes and Code Block nodes eliminates the need for List.Map and List.Combine in many situations
- Send to Web: formerly known as Share Workspace, we have improved the ability to view and interact with Dynamo online with Customizers
- File Export:  Users can now author DWG files in the Translation section of Dynamo Studio.
- Direct Shape:  Dynamo in Revit 2017 can now take advantage of faster and more sophisticated direct shape creation.  In most cases, solid and surface geometry can be sent directly into the Revit environment as smooth (rather than tesselated) surfaces and solids, categorized to whatever is needed.  In the cases where a smooth element cannot be created, a tesselated (mesh) object is created, as was the case previously.

Bug Fixes
- An extensive list can be found here: http://dynamobim.org/incoming-bug-fixes-for-dynamo-1-0-0

Known Issues
- Listed here: https://github.com/DynamoDS/Dynamo/wiki/Known-Issues

### 0.9.1 ###

Dynamo Core
- Direct manipulation: Sometimes numerical manipulation isn’t the right approach. Now you can manually push and pull Point geometry when in navigating in the background 3d preview.
- Freeze Functionality: When you have long running portions of your graph, or don’t want to export data to other applications, or want to debug some logic, don’t unplug your nodes. Now you can suspend execution of specified nodes in the graph by using Freeze in the right-click contextual menu
- Search Enhancements: Only look at the node libraries you want to with new filtering tools.  See more options at once by using a compact view, or get more information with the detail view.
- Zoom re-center: Select a node, then zoom and recenter your orbit on it in the 3d Preview Navigation
- CNtrl-drag:  Copy/Paste nodes in a familiar way
- Add comments to custom node inputs and full default states for complex data types
- More forgiving DesignScript syntax:  Users can now write instance methods (ex. MyCurve.PointAtParameter(0.5)) as Static Methods (ex. - Curve.PointAtParameter(MyCurve, 0.5))

Known issues
- No backwards compatibility with 0.9.0 and before. This is due to neccessary changes to the Dynamo API in advance of 1.0. These changes can be found in the [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes) document
- In some situations, placement of Adaptive components requires a change in list structure.  The AC placement nodes now expect to receive lists of lists of placement coordinates.  In the past, the nodes expected to only place one AC, now it expects to place many. If you are going to only place a single component, it needs to be nesting into a list.
- With Win10 the Dynamo Background Preview is blank. If your Win10 workstation contains a graphics card that used to work with Dynamo running Win7 or 8 and you experience an inability to render graphics you may wish to consult:

  https://github.com/helix-toolkit/helix-toolkit/issues/257#issuecomment-194145932

Dynamo Studio
- Share your work online:  Share interactive parametric models online.  Just publish your Dynamo graph and send a link to your colleagues or the whole world.  People can view and interact with your designs in a regular web browser with no Dynamo installed
- ImportExport: Read directly from DWG files and only pull out those pieces of the file that you want.  
- When a user downloads a dyn from the Customizer (or Shared Workspace), the dyn's Run setting is automatically set to "Manual". This may be confusing to some users when they open the dyn in Dynamo and see all Nulls in the outputs: simply click the Run button.
- Users have been reporting that the Customizer (Shared Workspaces) functionality is missing in Studio 0.9.1. If this is happening to you, please try uninstalling Studio 0.9.1 and reinstalling it. We are aware of the bug and a fix will be available soon. Please reach out to us if you experience any other related issues.


Dynamo for Revit
- Batch placement of adaptive components : Huge improvements to the speed and reliability of placing large numbers of adaptive components. Note that the nodes now expect lists of lists as inputs, so you may have to update your 9.0 graphs.


### 0.9.0 ###

Create DirectShape Elements in Dynamo
- You can now wrap meshes, solids, and surfaces in a DirectShape and place it into your Revit Model

Library Enhancements
- The library is now organized in a tree view to make it easier to find the nodes you want. Node types have also been color-coded to make it easier for you to locate create, action, and query nodes in the library

Manage Custom Node and Package Paths  
- Add paths to makes nodes and packages show up in the library

Node Layout Cleanup Improvements
- Clean up layouts considering groups as a whole or clean up layouts within groups

Background Preview works on Remote Desktop and Parallels
- For remote systems with GPUs and virtual machines with hardware-acceleration, background preview is now visible.

Additional Updates and Improvements
- New Chapters and expansion of the [Dynamo Primer](http://dynamobim.com/learn/)
- Easier to read Search Results 
- Node to Code Stabilization
- Improved 'Canvas Snapshot' functionality
- Move to .NET 4.5
- Lots of bug fixes

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
- Now localized to 12 languages (Dynamo for Revit and Dynamo standalone)
- Define the localization for Dynamo standalone [Instructions here](http://dynamobim.com/multilingual-dynamo/)

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

