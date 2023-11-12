### Dynamo Shaders:

This folder contains the following:
* some common structure definitions in hlsl taken from Helix 2.15
* some custom shaders for Dynamo meshes (1 vertex shader and 1 pixel(or fragment) shader) - these are used to draw Dynamo meshes.
* a bat script which compiles these shaders into binary blobs that further get compiled into DynamoCoreWPF.dll

### What are these files

* hlsl is high level shader language - a shader language for writing shaders for directX (windows)

### What are shaders

* shaders can perform many operations - but their most common use is to transform the vertices of a geometrical mesh and to draw the resulting triangles. The vertex shader is run first passing data to the fragment shader - which usually outputs a color - each color corresponds to a pixel on screen. Each shader is executed many times concurrently by the GPU.

### How do I update these shaders?

1. Write some code in the hlsl files.
2. Run the bat script from the command prompt or in the _...\src\DynamoCoreWpf\ViewModels\Watch3D\shaderSource folder_.
3. Recompile dynamo with a clean and rebuild.
4. Profit.

### What if I'm missing the FXC Compiler? 

If you experience missing fxc errors (_'fxc' is not recognized as an internal or external command_), you will need to install the fxc compiler via the [Windows SDK](https://developer.microsoft.com/en-us/windows/downloads/sdk-archive/), locate _fxc.exe_ and add it's location to your System Environment Variables (System Properties --> Advanced --> Environment Variables --> User variables for <username>). Example fxc.exe path added: _C:\Program Files (x86)\Windows Kits\10\bin\10.0.20348.0\x64_

### How to update these shaders when our Helix dependency is updated.

When Helix is updated, it's possible that the common buffers and shader definitions which our shaders rely on are updated - 
so we should make sure to update the referenced files here and recompile our shaders to make sure they still work.
The Helix shader definitions are here:
https://github.com/helix-toolkit/helix-toolkit/blob/develop/Source/HelixToolkit.Native.ShaderBuilder/PS/psSSAO.hlsl

WE SHOULD NOT MODIFY THESE DEFINITIONS - that makes maintenance/updating/consuming new Helix versions later much harder.

### What graphical magic do these shaders do - why do we need custom shaders?

These shaders are used to support the various rendering modes that Dynamo uses to display geometry. As a user interacts and creates geometry they may currently set the following flags.

* Selected
* Isolated
* Frozen
* FlatShade
* Special (or meta) Geometry - like a Gizmo
* Transparent
* VertexColors

Some of these flags can be enabled at the same time.
Each provides some modification of the expected visual output.
Here are some examples:

|  Mode | Alpha Effect  | Color Effect  | Interaction Notes  | Shading Effect   |   |   |
|---|---|---|---|---|---|---|
| Selected  | none  | blueish (regular colors when isolated)  |  overrides all other states - but interacts with isolated |   |   |   |
| Isolated  | * .1  | none  | none  | when isolated geometry is transparent,  when isolated  and selected - it has full alpha |   |   |
| Frozen  | *.5  |  | none  | none  |   |   |
| FlatShade  | none | none  |  none | uses texture colors with no lighting at all. (unlit)  |   |   |
| Special  | none  |  none  | none  | potentially unlit  |   |   |
| Transparent| ?  |   |   |   |   |   |
| VertexColors | ?  |   |   |   |   |   |

