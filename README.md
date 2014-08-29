
Feature Comparison
-----
The following table gives comparisons among more prominent features between ```Helix 3D``` and ```Bloodstone``` viewers. There may be more functionalities provided by ```Helix 3D``` but here we only outline those that are more user facing.

| Feature Name | Helix 3D | Bloodstone |
| ------------ |:--------:|:----------:|
| Zoom to fit | Yes | Yes |
| Zoom with mouse | Yes | Yes |
| Pan | Yes | Yes |
| Rotate | Yes | Yes |
| Off-centered rotation | No | Yes |
| Grid lines | Yes | No |
| UCS icon | Yes | No |
| Text support | Yes | No |
| Selection highlight | Deferred | Real-time |
| Phong shading | Yes | Yes |
| Phong shading light sources | 3 | 1 |
| Flat shading | No | Yes |
| Per-vertex color | No | Yes |
| Per-node color | No | Yes |
| Anti-aliasing | Yes | Yes |

Prioritized Task List
-----
The following list represents what are to be implemented in Bloodstone for it to reach feature parity with that of ```Helix 3D``` viewer. These tasks are not exhaustive and will grow as demanded:

- [x] Implement pan operation
- [x] Implement zoom operation
- [x] Implement zoom extent with double-click
- [ ] Replace ```IGraphicsContext::ActivateShaderProgram``` with ```IShaderProgram::Activate```
- [ ] Replace ```IGraphicsContext::RenderVertexBuffer``` with ```IVertexBuffer::Render```
- [ ] Encapsulate things like ```controlParams``` in impl-specific classes
- [ ] Remove APIs to deal with vertex/fragment shader from IGraphicsContext
- [ ] Remove ```alpha``` uniform from Phong fragment shader
- [ ] Introduce a notion of "inactive" shader program (those that fail compilation)
- [ ] Set the background Cornflower blue to match the geometry contents
- [ ] Output diagnostic information on Dynamo
- [ ] Fix creation failure for Parallels
- [ ] Only zoom-to-fit when geometries arrive for the first time
- [ ] Make Z-axis as up vector instead of Y-axis
- [ ] Align view operations with that of Revit (i.e. mouse actions etc.)
- [ ] Integrate zoom operation with mouse wheel
- [ ] Performance: Merge the two-pass conversion from IRenderPackage to VB
- [ ] Update Phong shader to include 3 light sources, and increase the saturation
- [x] Perform [proper OpenGL context creation](http://www.opengl.org/wiki/Creating_an_OpenGL_Context_(WGL)#Proper_Context_Creation)
- [x] Implement anti-aliasing for better visual quality
- [x] Implement geometry clearing when document is opened/closed/created
- [x] Implement geometry clearing when nodes are deleted

Screenshots
-----
Stadium designed by @elayabharath
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/eb-stadium-v0.png)

Another stadium designed by @elayabharath
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/eb-stadium-v1.png)

The graphics system always chooses the best anti-aliasing mode if the hardware allows it. If the default of 8x MSAA fails, the graphics context will fall back to 4x MSAA (or no anti-aliasing if even that fails). The following image makes the comparison among these settings (click on the image to display the original size for better comparisons):
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/multisample-anti-aliasing.png)

Color setting is now available on per-node basis
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/node-and-primitive-colors.png)

Colored spheres in a grid
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/colored-spheres.png)

Render style settings on a per-node basis, with default set to ```Phong Shading```
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/render-style-phong-shading.png)

Render style of ```Primitive Color``` allows color specified on triangles to show through without shading
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/render-style-primitive-color.png)
