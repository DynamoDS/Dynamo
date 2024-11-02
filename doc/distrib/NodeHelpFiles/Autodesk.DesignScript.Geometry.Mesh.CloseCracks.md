## In-Depth
`Mesh.CloseCracks` closes cracks in a mesh by removing internal boundaries from a mesh object. Internal boundaries can arise naturally as a result of mesh modeling operations. Triangles can be deleted in this operation if degenerate edges are removed. In the example below, `Mesh.CloseCracks` is used on an imported mesh. `Mesh.VertexNormals` is used to visualize the overlapping vertices. After the original mesh is passed through Mesh.CloseCracks, the number of edges is reduced, which is also evident by comparing the edge count, using a `Mesh.EdgeCount` node. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)