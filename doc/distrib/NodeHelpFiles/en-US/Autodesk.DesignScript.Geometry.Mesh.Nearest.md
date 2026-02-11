## In-Depth
`Mesh.Nearest` returns a point on the input Mesh that is nearest to the given point. The point returned is a projection of the input point onto the Mesh using the normal vector to the mesh passing through the point resulting in the nearest possible point.  

In the example below, a simple use case is created to show how the node works. The input point is above a spherical Mesh, but not directly on top. The resulting point is the closest point that lies the mesh. This is contrasted with the output of the `Mesh.Project` node (using the same point and mesh as inputs together with a vector in the negative 'Z' direction) where the resulting point is projected onto the mesh directly below the input point. `Line.ByStartAndEndPoint` is used to show the 'trajectory' of the projected point onto the Mesh.

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.Nearest_img.jpg)