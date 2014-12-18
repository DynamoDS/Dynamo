![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png) 
Dynamo is a visual programming tool that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages.


## Get Dynamo ##

Looking to learn or download Dynamo?  Check out [dynamobim.org](http://dynamobim.org/learn/)!


## Build ###

You'll need Visual Studio and a git client to build Dynamo.  Find more about how to build Dynamo at our [wiki](https://github.com/DynamoDS/Dynamo/wiki).


## Contribute ##

Dynamo is an open-source project and would be nothing without its community.  You can make suggestions or track and submit bugs via [Github issues](https://github.com/DynamoDS/Dynamo/issues).  You can submit your own code to the Dynamo project via a Github [pull request](https://help.github.com/articles/using-pull-requests).  


## Releases ##

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


## Dynamo License ##

Those portions created by Ian are provided with the following copyright:

Copyright 2014 Ian Keough

Those portions created by Autodesk employees are provided with the following copyright:

Copyright 2014 Autodesk


Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

## Instrumentation ##
Dynamo contains an instrumentation system that anonymously reports usage data to the Dynamo team. This data will be used to enhance the usability of the product. Aggregated summaries of the data will be shared back with the Dynamo community.

An example of the data communicated is:

"DateTime: 2013-08-22 19:17:21, AppIdent: Dynamo, Tag: Heartbeat-Uptime-s, Data: MTMxMjQxLjY3MzAyMDg=, Priority: Info, SessionID: 3fd39f21-1c3f-4cf3-8cdd-f46ca5dde636, UserID: 2ac95f29-a912-49a8-8fb5-e2d287683d94"

The Data is Base64 encoded. For example, the data field above ('MTMxMjQxLjY3MzAyMDg=') decodes to: '131241.6730208' This represents the number of seconds that the instance of Dynamo has been running. 

The UserID is randomly generated when the application is first run. The SessionID is randomly generated each time Dynamo is opened.


## Third Party Licenses ##

###Avalon Edit###
http://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor  
http://opensource.org/licenses/lgpl-3.0.html  

###CSharpAnalytics###
https://github.com/AttackPattern/CSharpAnalytics  
http://www.apache.org/licenses/LICENSE-2.0

###Helix3D###
https://helixtoolkit.codeplex.com/  
https://helixtoolkit.codeplex.com/license  

###Iron Python###
http://ironpython.net/  
http://opensource.org/licenses/apache2.0.php  

###Kinect for Windows###
http://www.microsoft.com/en-us/kinectforwindows/  
http://www.microsoft.com/en-us/kinectforwindows/develop/sdk-eula.aspx 

###Microsoft 2012 C Runtime DLLS, msvcp110.dll and msvcr110.dll###
http://msdn.microsoft.com/en-us/vstudio/dn501987

###Moq###
http://www.nuget.org/packages/Moq/  
http://opensource.org/licenses/bsd-license.php

###MiConvexHull###
http://miconvexhull.codeplex.com/  
http://miconvexhull.codeplex.com/license  

###NCalc###
http://ncalc.codeplex.com/  
http://ncalc.codeplex.com/license  

###NDesk Options###
http://ndesk.org/Options#License

###Newtonsoft JSON###
https://github.com/JamesNK/Newtonsoft.Json  
https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md

###NUnit####
http://www.nunit.org/  
http://www.nunit.org/index.php?p=license&r=2.6.2  

###OpenSans font from Google###
http://www.google.com/fonts/specimen/Open+Sans  
http://www.apache.org/licenses/LICENSE-2.0.html

###Prism###
http://msdn.microsoft.com/en-us/library/gg406140.aspx  
http://msdn.microsoft.com/en-us/library/gg405489(PandP.40).aspx  

###Revit Test Framework###
https://github.com/DynamoDS/RevitTestFramework  
http://opensource.org/licenses/MIT

###Lucene.Net###
http://lucenenet.apache.org/
http://www.apache.org/licenses/LICENSE-2.0
