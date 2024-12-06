## In-Depth
`Mesh.VerticesAsThreeNumbers` determines the X, Y, and Z coordinates of the vertices in a provided mesh, therefore resulting in three numbers per vertex. In the example below, `Mesh.Cuboid` and `Number.Slider` are used to create a cuboid mesh, which is then used as input to determine the coordinates of each vertex. In addition to this, `Mesh.Vertices` is used to report the list of coordinates per vertex as well as to show the vertices in the preview. As shown in the example, the value reported by `Mesh.VerticesAsThreeNumbers` will be three times the `Mesh.VertexCount` value.

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.VerticesAsThreeNumbers_img.jpg)