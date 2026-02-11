## In-Depth
`Mesh.GenerateSupport` node is used to add supports to input mesh geometry in order to prepare it for 3D printing. Supports are required to successfully print geometry with overhangs to ensure proper layer adhesion and prevent the material from sagging during the printing process. `Mesh.GenerateSupport` detects overhangs and automatically generates tree-type supports that consume less material and can be easier removed, having less contact with the printed surface. In cases where no overhangs are detected, the result of the` Mesh.GenerateSupport` node is the same mesh, rotated and into an optimal orientation for printing and translated to the XY plane. The configuration of supports is controlled by the inputs:
- baseHeight - defines the thickness of the lowest part of the support - its base
- baseDiameter controls the size of the base of the support
- postDiameter input controls the size of each support in its middle
- tipHeight and tipDiameter control the size of supports at their tip, in contact with the printed surface
In the example below, `Mesh.GenerateSupport` node is used to add supports to a mesh in the shape of the letter ‘T’.

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)