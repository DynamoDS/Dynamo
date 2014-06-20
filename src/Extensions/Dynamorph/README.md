![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png) 

Dynamorph, morph the heck out of your design.

## Tasks (Prioritized) ##

##### Core Tasks #####
- [x] Implement first cut Camera class
- [x] Enable shader uniform passing from C++
- [x] Update background color fill to RGB(30, 30, 30)
- [x] Use GLM for transformation matrix computation
- [x] Add bounding box to IVertexBuffer class
- [x] Fit camera to bounding sphere of IVertexBuffer
- [x] Enabled depth test while rendering
- [x] Fix bounding sphere calculation issue
- [x] Implement SynthesizedGraph/Node/Edge classes
- [x] Implement topological sort for synthesized graph
- [x] Provide helper to synthesize Dynamo graph 
- [x] Implement layout algorithm for synthesized graph
- [x] Implement rendering for synthesized graph
- [x] Add support for Point, Line, LineStrip primitive types
- [x] Add normal information in vertex data
- [x] Use layout qualifier in GLSL instead of hard-coding
- [x] Implement Phong shading model in GLSL
- [x] Add slider to work with synthesized graph
- [x] Design structure to keep node-geometry data
- [x] Add shader uniform to control per-vertex alpha
- [x] Enable slider to work with C++ Visualizer
- [x] Implement Track Ball class for navigation
- [x] Make nodes align to leaf nodes instead of root nodes
- [ ] Handle node deletion to clear geometry data

##### Miscellaneous Tasks #####
- [x] Move shader source to external files
- [x] Introduce "normal matrix" into [Phong shader](http://www.mathematik.uni-marburg.de/~thormae/lectures/graphics1/code/WebGLShaderLightMat/renderer.js)
- [x] Remove normal computation in GetTriangleGeometries
- [x] Update aspect ratio when window is sized
- [ ] Make slider snap to center point of a column of nodes
- [ ] Translate directly into GeometryData as interleaved
- [ ] Factor mpNodeGeometries, mpGeomsOnDepthLevel out of Visualizer
- [ ] Fix: Line segment with 0 vertex should be filtered
- [ ] Fix: Warning surrounding deprecated GLM functions
- [ ] Merge multiple RenderPackage into one vertex buffer
- [ ] Fix: Dynamorph relaunch problem
- [ ] Fix: Closing Dynamo does not close Dynamorph
- [ ] Implement logging mechanism
- [ ] Incorporate [debugging facility](http://www.opengl.org/registry/specs/ARB/debug_output.txt) provided by OpenGL
- [ ] Display OpenGL logs in Dynamorph window
- [ ] Implement mini-ball for tightest bounding box
- [ ] Add GLM licence terms to EULA and ReadMe.md
- [ ] Implement hit testing for graph visuals
- [ ] Fix: Wrong upstream node count with adjacent edges
- [ ] Fix: Update methods to take BoundingBox & (not *)

##### Obsolete Tasks #####
- [x] Implement saving of tessellated data to bin file
- [x] Implement loading of tessellated data from file
- [x] Replace "CameraConfiguration::eye" with "viewDirection"


## Screenshot Of The Day ##
This is the latest screenshot of Dynamorph. It will be updated as and when there is any visible change to Dynamorph to reflect the latest development.

#### Point, line primitives now rendered with precise colors! :) ####
![Image](https://raw.githubusercontent.com/Benglin/Dynamo/Recharge_Ben/src/Extensions/Dynamorph/dynamorph-screen.png)
