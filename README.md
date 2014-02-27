![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png) 

#Visual Programming for BIM#

## Description ##
Dynamo extends the parametric functionality of Autodesk Revit. Dynamo aims to be accessible to both non-programmers and the programmers alike with the ability to visually script behavior, define custom nodes, and script using Python.

## Contributors ##

* [Ian Keough](https://github.com/ikeough): Started the project, main developer.
* [Stephen Elliott](https://github.com/Steell): Engine overhaul, main developer. 
* [Peter Boyer](https://github.com/pboyer): UX and UI overhaul, main developer.
* [Lev Lipkin](https://github.com/LevL): Revit node designer, Revit interaction specialist.
* [Matt Jezyk](https://github.com/tatlin): Product management and requirements gathering. Node designer and overall design input.
* [Zach Kron](https://github.com/kronz): Project management and requirements gathering.
* [Luke Church](https://github.com/lukechurch): Software Architecture
* Lillian Smith: Useful feedback and use cases.
* [Tom Vollaro](https://github.com/tvollaro): Useful feedback and use cases.


Dynamo has been developed based on feedback from several parties including Arup, KPF, Buro Happold, Autodesk, and students and faculty at the USC School of Architecture.


## Running Dynamo ##

The current version will run on top of Revit 2013, Revit 2014, and Project Vasari Beta 3. 

## Releases ##

###0.6.3###

February 2014

New

- Dynamo Sandbox:   Explore Dynamo without Revit or Vasari installed (Dynamo Installation folder>Core>DynamoSandbox.exe)
- Application level settings for Imperial and Metric Units
- Daylighting with cloud Rendering service sample files
- Set Parameters Node (not restricted to Families)
- Add name to reference plane node.
- Convert to Unitzed measures (Length, Area, Volume)
- Explode Node (Solids to Faces, Faces to Edges) 
- Solids from Elements handles lists
- Select All Elements of Type and Category Nodes
- Wall nodes element IDs persist after changes
- Area Node
- Volume Measure node
- Length from Curve Node
- Topography from Points and Points from Topography Nodes
- Last of List Node
- Fileter by Boolean Mask
- Group by Key Node
- Is Null node (for filtering out null values) 
- Explode Node (replaces Explode Solid)
- Python nodes now can take node inputs as functions
- Shuffle List Node
- Select All Elements of Category Nodes
- Divided Path Node updates
- XYZs from Divided path
- Treat curves and edges the same for intersection operations
- Preserve Wall Elements on change
- Improvments to node Help descriptions
- Toolbar shortcuts
- Improvements to Preview bubble display (fades, compact error messages, etc)

Fixes

- More robust handling of null values
- Better handling of educational licenses for Daylighting
- Fix for V subdivision in Divided Surface node 
- Fixed bad equals comparison
- Fix for jumpy zoom controls
- Increased zoom limits
- Many fixes for Preview bubble alignments and appearance
- Many Fixes for Excel interoperability
- Allow unpluggeed ports on Perform All
- Sphere cannot be made anywhere but 0,0,0
- Crash when making revolved geometry


###0.6.2###

November 2013

Fixes
- 2*Pi node
- Cross Product lacing now gives a list of lists
- Lots of custom node fixes
- Fixes for package manager uploads (crash when not logged in)
- Python cleanup and fixes
- Dynamo now handles close and open new document in same session
- Get Family Location fixes
- Color range node preview is now accurate
- Structural Framing can now take a single or a list of UP values
- So Many!  Check Github for full list


New
- UI enhancements
- Mouse-less navigation
- Background Preview elemnents highlight when nodes are selected
- Selection of Solids in background highlights nodes
- Cloud based Daylighting Analysis
- No limit to the size of canvas
- Background 3d visualization improvments
- Resizable Watch 3d windows
- PReview data bubbles on nodes
- Added an interval node to Web Request
- Equal can now compare all data types
- Updates to arduino. Use delimiter instead of new lines.
- More forgiving inputs for Solid Geometry creation
- Browser reorganization.  
- Search Improvements
- List handling.  Most nodes now can take lists and lists of lists
- XYZ and Vector improvments:  Normalize, Dot product, components, ploar and spherical coordinates
- Solid Primitives: Boxes, Cylinder, Sphere, Torus
- Boolean improvments
- Get Family Location now takes single origin and multi-pick placement families
- Drag and Drop dyn files into canvas to open
- Asin, Acos, Atan
- Wall and Floor Creation nodes.  WARNING!  Recreated, not modified on change.
- Point and Curve numbering available in node right click "Show Label"
- Extract Transform Basis for x, y, and z vector components
- Curve Plane Intersection
- transform origin node
- plane from reference plane.
- line by start point direction and length
- integer slider
- xyz by distance offset from origin
- STL export (from file menu)
- Adaptive Component Batch creation node (make more stuff faster)
- Default values added to many nodes

###0.6.1###

October 2013

Fixes
- Project to Face/Plane corrections
- many excel node fixes and performance improvements
- Python output improvements for integers
- Best Fit Plane Orientation consistency
- Conditional statements don't break formula node
- Custom node fixes
- Package Manager download fixes


New
- Undo/Redo now available
- Many UI improvements
- Visualization in canvas (available in Vasari only)
- Background and watch 3d can now draw from any node with geometric output
- Coloring preview geometry based on node selection
- navigation without with key commands
- navigation with onscreen commands
- View creation nodes: section, axo, crop controls,element isolation, more
- Sheet creation and View placement
- Override Colors in View
- Package manager search is now instant, no commit needed
- Deprecation of Packages
- Drop down menus sorted by name
- name inputs for levels
- better reference line/model line creation methods
- Model Text nodes
- Raybounce nodes


###0.6.0###

September 2013

Fixes
- Nodes properly save/load port state
- Build sublists now uses same sematics as Number node
- Better descriptions for boolean logic nodes
- Fixed bug for equal distance on curve node for closed curves
- Fixed List To CSV to allow for non-string data
- Performance improvement for Combine node
- Nodes properly save/load port state
- Fixed cultural variaces issues with number node
- Number sequence and range fixes
- Better descriptions for boolean logic nodes
- Length node fixes
- Best Fit Plane fixes
- Legacy custom node loading fixes
- Watch node performance improvements
- Installer now installs for all users
- Better output ports for intersection nodes
- Excel nodes improvements


New
- Package Manager: share custom nodes online
- Can now use material parameters (Get Material by Name node)
- Get and Remove From List nodes now can use ranges of indices
- Color Range node
- Intersection nodes return more granular results
- Adaptive Component By Face and By Curve nodes 
- Default values for some nodes
- Root finding nodes
- Convert anything to a String (To String node)
- Is XYZ Zero Length & XYZ Length nodes
- New Slice List node (get a sublist from a given list)
- Custom nodes differentiated from built in nodes in search
- Domain node (specify a domain by Min and Max)
- For Each node
- Multithreading nodes
- Nodes to perform explicit lacing (Lace Shortest, Longest, Cartesian)
- Divided Surface Selection node (returns a list of hosted components)
- Add basic instrumentation infrastructure to report usage
- Allow writing a range of data to excel
- Face Face intersection node
- Default value capability for ports
- UI Refinement


###0.5.3###

July 2013

Fixes
- XYZ Array on Curve returns a point on the end
- Conditional node improvements
- Copy Paste notes now works
- Loft node fixes (now takes non-planar curves)
- Reference Point by Normal and Distance
- Fixes for Get Paramter node
- fixed case sensativity issue with Forumla node
- fixed crash with Create node from selection functionality
- fixes for Drop and Take nodes
- Angle node now saves properly
- Removed lacing for Get and Remove from List nodes


New
- Select Elements nodes (box select any collection of Revit Elements)
- Search Bar is more expandable
- Double Click edit for all editable nodes
- Node Renaming (edit the visible name in a node instance)
- Axonometric View Creation
- New Tutorials on https://github.com/ikeough/Dynamo/wiki/Learning-Dynamo
- Open Maipulate and Save Excel files
- Save View node
- e Node 
- Formula node recognizes Pi
- Number node now supports sequences and range inputs
- True for All and True For Any nodes
- Tooltips for Custom Node inputs 
- Apply Function to List node
- Remove from List, Remove Every Nth, Shift List Indeces nodes
- Project Point on Face works for Planes now
- Equal Distance XYZs on Curve node
- Get Active View Node
- Smooth Node (running average for a numberical output)

###0.5.2###

July 2013

Fixes
- UV Grid improvements
- Particle System and Dynamic Relaxation improvements
- Delete for notes
- Formula node is now case insensitive
- Watch and Watch 3d improvments
- Zoom and workspace tabbing
- Length node culture formatting
- Fixed rectangle node
- Custom node fixes
- Multi-output node fixes

New
- Structural Framing nodes
- More List management nodes, Flatten changes, Repeat node
- More Math Nodes
- Node alignment tools 
- Best Fit Plane, Best Fit Arc
- Number Slider state display
- Last computed value is now displayed on nodes with mouse-over
- Zoom state saved with workspaces
- More nodes for using Analysis Visualization Framework (model coloring)
- Color Nodes (Hue, Brightness, Saturation, etc)
- Better node error state reporting
- Curves Through Points node
- Find Nodes from Selected Elements
- Watch node shows index of list members
- Tooltips in search

Known Issues
- See Issues on Github:  https://github.com/ikeough/Dynamo/issues?state=open

###0.5.0###

June 2013

Known Issues
- Watch Node results disappear when switching views to a different workspace tab. Issue #156
- Cannot cancel out of a face selection operation.  Issue # 155
- Noise Field and Color Brightness Nodes Cannot Recieve List Inputs #150
- "XYZ Array on Curve" Node does not create point on end of curve.  #138
 
Fixes
- Better crash handling
- Fixed preview geometry crashes
- Python script node crash fixes
- Save as saves to new location properly
- Custom node creation fixes (save as, reopen, recursive node issues)
- Fixed Write CSV File node
- Loading files with missing custom nodes creates a proxy node indicating the node they are replacing
- Sliders update values properly when range is changed
- Copy/Paste fixes
- Evaluate curve or edge no longer crashes Watch 3d
- Better handling of escape characters in string nodes
- Better handling of localized handling of comma and period decimal delimiters
- Nodes using selection remember associations after save and reopen
- List not first input is now index0
- Fixed Build Sequence node crash
 

New
- Combined installer for Revit 2013, 2014, and Vasari Beta 2 and 3
- Better background contrast with node wires and other visibility improvements
- Improved handling of lists by many nodes
- Dyn file load time improvements
- New Tessellate nodes:  Convex Hull 3d , Delaunay on Face and 3d, Voronoi on Face
- Background preview controls
- More solid and curve creation nodes
- String nodes now handle returns, tabs, and other escape characters
- Compose Functions Node (combine two single parameter functions into one)
- Filter Out Node (removes a given predicate from a list)
- Transpose List Node (swaps rows and columns in a list of lists)
- Combine operates on uneven lists
- Copy/Paste copies node state
- Length node that uses Project Units (metric and imperial)
- Preview visibility controls in right click for each node



###0.4.0###

June 2013

Known Issues
- Cannot use comma as a decimal separator
- Node Preview state is not saved with dyn file
- Transform Point does not respond to rotation input

Fixes
- Can now run Python scripts from disk location and embedded
- Autocomplete for Python now works in Vasari
- Deluany Tessellation node is working 
- Preview 3d geometry in the view background refreshing properly
- Formula node can now copy paste
- Fixed crash bug with connecting node to itself. This also corrects the double click crash on a port.
- Fix XYZFromReferencePoint so that it returns an XYZ.
- Fixed behavior where lacing and element count would not coincide, sometimes leaving an element behind.
- CurveByPointsByLine no longer duplicates points.
- Fixes for Adaptive Component placement
- Better crash handling
- Fixed multiple crash issues around deleting elements created by graph
- Auto-Mapping (lacing)improvments
- Selection nodes improved save/open fidelity
- Experimental nodes for Isometric View and Section Box generation
- Fixed Crash Saving Image

New
- Wiring new inputs will push out old inputs
- Iron Python 2.7.3 bundled with install
- OffsetCurves node
- Update CurveByPoints node to allow for creation of reference lines
- Reference Curve Node
- More list management nodes: sublist, slice,  flatten
- Preview geometry is now controlled on a per-node basis (right click menu)
- Compute curve derivatives
- Open file defaults to sample directory
- UI improvements and cleanup
- Cache previous position of Dynamo window, do not maximize on startup
- Add XYZStartEndVector, CrossProduct, Negate, and Average
- Normalize the vectors when creating a transform
- Lacing is set to "Longest" by default
- Angle Node that takes degree inputs
- Updates, fixes, and culling of Samples
- Nodes for CurveLoop from Curves, Curve List from Curve Loop
- Nodes for Revolve and Sweep for Solid Creation


###0.3.4###

May2013

Known Issues
- Cannot run Python scripts from disk location (embedded is fine)
- Delauny Tesselation node is always passing an empty list
- Preview 3d geometry in the view background is not consistently refreshing
- Formula node crashes when copy pasting when running in auto


Fixes
- Dynamo now works alongside Revit Python Shell and other conflicting addins
- Solar Radiation example works
- More stability


New and Updated Nodes
- Adaptive Components
- Formulas using N-Calc syntax
- Compute Face Derivatives
- Project Points on Curves and Faces
- Extract Solids, Faces, and Edges from elements
- Evaluation of Curves and Edges
- Selection of Imports, Host objects, Edges, Faces and Solids from Element
- Watch3d improvements
- Python node Autocomplete (in progress and only in Revit)

Functionality and UI
- Appearance Cleanup
- Preview geometry in Dynamo Background 
- Category and Node Browsing Improvements
- Ability to pass lists into nodes (Lacing and auto-mapping of lists)
- New Node appearance
- Graph retains memory and parametric control of elements created in previous sessions
 

Engineering
- MVVM standardization

Samples
- Adaptive Component Placement
- Face Extraction from Solids
- Formulas
- Create Point Sequence
- Some existing sample cleanup


###0.3.0###

April 2013

Known Issues
- Dynamo cannot start with Revit Python Shell Installed.
- Dynamo continues regenerating revit elements after closing when "Run Automatically" is checked
- Face selection of loaded families move origin to Revit 0,0,0
- Solar Radiation Optimization Sample may lock CSV file and stop running


New or Updated nodes
- Watch 3d geometry preview
- Expanded Curve drawing nodes
- Transforms
- Vectors
- More Analysis capabilities
- Delaunay Tessellation
- Dynamic Relaxation 
- Leap Motion interface
- Drafting view
- Syntax coloring for Python Node Editor
- Random Number generator
- Simplex Fields
- Read Image from File node
- Arduino node is more robust
- UDP node for responding to input over network 
- UV Grids
- Surface Domain nodes
- More math nodes (modulo and exponent)
- Height node
- Dynamic Relaxation improvments

UI:
- Custom Node creation from Selection
- Multiple Outputs from custom nodes
- Cut and Paste 
- Box and Multi Select
- Search/Browse: Scalable browsing, more robust search
- Lacing: Data Matching for Lists
- Application Menu standardization
- Spline or Polyline Connector option
- More legible and compact node display

Engineering:
- Code Cleanup
- Expanded code Documentation
- Code Separation (Disentangle UI, Engine, Revit code)
- API reflection:  Ability to automatically generate nodes from the API
- Automated testing for improved stability

Samples:
- Dynamic Relaxation
- Curves
- Arduino
- CSV driving point creation
- Tesselation
- Transforms




###0.2.0###

November 2012

New or updated nodes:

- Added slider value display when slider is moved
- Added XYZ utility nodes (XYZ Zero, XYZ Basis, XYZ Scale, XYZ Add, Evaluate Normal, Evaluate XYZ)
- Added XYZ Number Extractor nodes (XYZ -> X, XYZ -> Y, XYZ -> Z)
- Added Reference Point By Normal node
- Fixed Arduino node, it now works with new framework
- Added Execution Interval (Timer) node
- Added Spatial Field Manager node and Analysis Results node
- Added Level node and Level by Selection node
- Added Planar Nurbs Spline node
- Added Divided Surface node
- Added Divided Path node
- Updated Lofted Form node to handle surfaces vs solids and solids vs voids
- Added Chomp, Dice, LaceForward and LoacBackwards definitions for working with 2D arrays.
- Added new string utility nodes 
- Added a CSV file writer and basic file writer nodes
- Added a Watch node
- Added Dynamic Relaxation node (still experimental)

UI:
- Added abilty to create Notes in graphics window
- The built-in samples in File/Samples have been reorganized to use subfolders
- added File/Save command
- Pressing the Dynamo Ribbon button in Revit/Vasari while Dynamo is already running now simply makes Dynamo window visible again
- It is now possible to move around the graphics screen with the arrow keys.
- If second monitor is available, Dynamo will maximize to it.

###0.1.0###

June 2012

- User Interface - side panel to allow you to search and then drag and drop new nodes in to place
- User-created nodes - you can make 'sub-nodes' and then reference them elsewhere, these act like writing a reusable function in a programming language
- Python Scripting node - we have integrated a python scripting node into 
- Math, logic and other nodes - many new nodes added to support evaluation,  iteration and looping

Dynamo For Vasari Beta 2 WIP: Integrating Dynamo into Project Vasari and extending Dynamo to support integrated analysis and performance-based design.


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
Dynamo now contains an instrumentation system. This anonymously reports usage data to the Dynamo team that will be used to enhance the usability the product. Aggregated summaries of the data will be shared back with the Dynamo community.

An example of the data communicated is:

"DateTime: 2013-08-22 19:17:21, AppIdent: Dynamo, Tag: Heartbeat-Uptime-s, Data: MTMxMjQxLjY3MzAyMDg=, Priority: Info, SessionID: 3fd39f21-1c3f-4cf3-8cdd-f46ca5dde636, UserID: 2ac95f29-a912-49a8-8fb5-e2d287683d94"

The Data is Base64 encoded. For example, the data field above ('MTMxMjQxLjY3MzAyMDg=') decodes to: '131241.6730208' This represents the number of seconds that the instance of Dynamo has been running. 

The UserID is randomly generated when the application is first run. The SessionID is randomly generated each time Dynamo is opened.




## Third Party Licenses ##

###Avalon Edit###
http://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor  
http://opensource.org/licenses/lgpl-3.0.html  

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

###NUnit####
http://www.nunit.org/  
http://www.nunit.org/index.php?p=license&r=2.6.2  

###OpenSans font from Google###
http://www.google.com/fonts/specimen/Open+Sans
http://www.apache.org/licenses/LICENSE-2.0.html

###Prism###
http://msdn.microsoft.com/en-us/library/gg406140.aspx  
http://msdn.microsoft.com/en-us/library/gg405489(PandP.40).aspx  


###ASM v. 220###
© 2013 Autodesk, Inc.  All rights reserved.   


All use of this Software is subject to the terms and conditions of the Autodesk license agreement accepted upon installation of this Software and/or packaged with the Software. 



Trademarks  


Autodesk and T-Splines are registered trademarks or trademarks of Autodesk, Inc., and/or its subsidiaries and/or affiliates.  


Intel, Xeon and Pentium are registered trademarks or trademarks of Intel Corporation or its subsidiaries in the United States and other countries.  


Spatial, ACIS, and SAT are either registered trademarks or trademarks of Spatial Corp. in the United States and/or other countries. 


D-Cubed is a trademark of Siemens Industry Software Limited. 


Rhino is a trademark of Robert McNeel & Associates. 


All other brand names, product names or trademarks belong to their respective holders. 



Patents 

Protected by each of the following Patents: 7,274,364 



Third-Party Software Credits and Attributions 

This software is based in part on the works of the following: 


ACIS® © 1989–2001 Spatial Corp. 



Portions related to Intel® Threading Building Blocks v.4.1 are Copyright (C) 2005-2012 Intel Corporation.  All Rights Reserved. 



Portions related to Intel® Math Kernel Library v.10.0.3 (www.intel.com/software/products/mkl) are Copyright (C) 2000–2008, Intel Corporation. All rights reserved. 



This work contains the following software owned by Siemens Industry Software Limited: 


D-CubedTM HLM © 2013. Siemens Industry Software Limited. All Rights Reserved. 

D-CubedTM CDM © 2013. Siemens Industry Software Limited. All Rights Reserved.  



This Autodesk software contains CLAPACK v.3.2.1. 

Copyright (c) 1992-2011 The University of Tennessee and The University of Tennessee Research Foundation.  All rights reserved. 

Copyright (c) 2000-2011 The University of California Berkeley. All rights reserved. 

Copyright (c) 2006-2011 The University of Colorado Denver.  All rights reserved. 


Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met: 


- Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 


- Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer listed in this license in the documentation and/or other materials provided with the distribution. 


- Neither the name of the copyright holders nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 


The copyright holders provide no reassurances that the source code provided does not infringe any patent, copyright, or any other intellectual property rights of third parties.  The copyright holders disclaim any liability to any recipient for claims brought against recipient by any third party for infringement of that parties intellectual property rights. 


THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 



This Autodesk software contains Eigen 3.2.0.  Eigen is licensed under the Mozilla Public License v.2.0, which can be found at http://www.mozilla.org/MPL/2.0/.   A text copy of this license and the source code for Eigen v.3.2.0 (and modifications made by Autodesk, if any) are included on the media provided by Autodesk or with the download of this Autodesk software.   
