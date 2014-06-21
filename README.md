![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png) 
Dynamo is a Visual Programming language that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages.

Dynamo runs as an addin on top of Autodesk Revit, extending the parametric functionality already provided.

## Contributors ##

* [Ian Keough](https://github.com/ikeough): Started the project, main developer.
* [Steve Elliott](https://github.com/Steell): VM, language, and node developer. Functional programming specialist.
* [Peter Boyer](https://github.com/pboyer): Package manager and geometry developer.
* [Lev Lipkin](https://github.com/LevL): Revit node designer, Revit interaction specialist.
* [Matt Jezyk](https://github.com/tatlin): Product management, requirements gathering, and node designer.
* [Zach Kron](https://github.com/kronz): Product management and requirements gathering.
* [Luke Church](https://github.com/lukechurch): Software Architecture, Programming UX specialist.
* [Aparajit Pratap](https://github.com/aparajit-pratap): Node and UI developer.
* [Ben Goh](https://github.com/Benglin): UI developer.
* [Elaybharath Elango](https://github.com/Elayabharath): Interaction and visual designer.
* [Jun Mendoza](https://github.com/junmendoza): Language and VM developer.
* [Monika Prabhub](https://github.com/monikaprabhu): Testing and QA.
* [Ritesh Chandawar](https://github.com/riteshchandawar): Testing and QA.
* [Sharad Jaiswal](https://github.com/sharadkjaiswal): Project management.
* [Yu Ke](https://github.com/ke-yu): Langauge and VM developer.
* [Randy Ma](https://github.com/Randy-Ma): Geometry developer.
* [Patrick Tierney](https://github.com/hlp): Geometry developer.
* [Colin McCrone' (https://github.com/mccrone): Technology Evangelist, Education, Requirements 


Dynamo has been developed based on feedback from several parties including Arup, KPF, Buro Happold, Autodesk, and students and faculty at the USC School of Architecture.


## Running Dynamo ##

The current version will run on top of Autodesk Revit 2013, Autodesk Revit 2014, and Autodesk Project Vasari Beta 3. 

## Releases ##

###0.7.1###
- Package Manager is Back
- 0.6.3 files will be upgraded to 0.7 format
- Experimental Library Loading
- Stability Improvments
- Revit Element created and Selected during a session will be rememebered in later sessions.
- Visualization Performance improvments

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
