## In-Depth
`Mesh.Cone` creates a mesh cone whose base is centered at an input origin point, with an input value for base and top radii, height and a number of `divisions`. The number of `divisions` corresponds with the number of vertices that are created at the top and base of the cone. If the number of `divisions` is 0, Dynamo uses a default value. The number of divisions along Z axis is always equal to 5. The `cap` input uses a `Boolean` to control if the cone is closed at the top.
In the example below, `Mesh.Cone` node is used to create a mesh in a shape of a cone with 6 divisions, thus the base and top of the cone are hexagons. `Mesh.Triangles` node is used to visualize the distribution of mesh triangles.


## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)