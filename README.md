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

The current version will run on top of Autodesk Revit 2013, Autodesk Revit 2014, and Autodesk Project Vasari Beta 3. 

## Releases ##

###0.7.0###
This is an alpha quality release which represents a significant refactoring of the underlying code.  There are some notable (temporary) restricitions to the functionality that is available in 0.6.3, and major enhancements to others. To allow users to continue work with 0.6.3 while exploring 0.7.0, this release can be installed side by side with older releases.  

New

- New geometry tools:  Dynamo now has a much more extensive collection of geometric operations that are available in stand alone mode as well as when running in Revit. 
- New scripting interface:  Dynamo now allows for direct input of DesignScript code into CodeBlock nodes.  Please see this document for learning DesignScript syntax and capabilities: http://designscript.org/manual.pdf  
 
 
Temporarily Unavailable Functionality from 0.6.3 (coming back soon!)

- Upgrade:  0.6 Dynamo files cannot be opened in 0.7.0.  We are actively working on the upgrade mechanism
- Package Manager: currently disabled until migration is working
- Recursion in custom nodes
- Revit Element explosion to geometry.  Currently users cannot generically extract geometric information from Revit elements.  However, there are different tools that, on a per elment basis, can extract geometric data.  For instance, After selecting a curve from Revit, a user can look in Revit>Element>CurveElement>Query>Curve to extract the geometry.  Similarly, a Family Instance can be queried for such items as faces, curves, location, etc.  
- Revit Elements created in a Revit session are not remembered in subsequent sessions.  New elements will be created when files are re-opened.
 
 
Known Problem Areas
- You can only load 0.7.0 or 0.6.3 in a Revit session.  You must close Revit before changing from one to the other.
- Visualization can be slow with lots of curvy stuff
- Manually Deleting and recreating Revit elements created by Dynamo can cause element duplication or failure to be re-created when the graph is re-run,
- Search tags are in progress.  If you don't find what you are looking for via search, try browsing.

 

###0.6.3###

March 2014

New

- Dynamo Sandbox:   Explore Dynamo without running Revit or Vasari (see your Start Menu)
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


###ASM v. 219###
© 2014 Autodesk, Inc.  All rights reserved.   


All use of this Software is subject to the terms and conditions of the Autodesk license agreement accepted upon previous installation of Autodesk Revit or Autodesk Vasari. 



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

