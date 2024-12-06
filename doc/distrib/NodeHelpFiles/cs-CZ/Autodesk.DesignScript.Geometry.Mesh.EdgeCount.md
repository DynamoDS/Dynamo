## In-Depth
This node counts the number of edges in a provided mesh. If the mesh is made of triangles, which is the case for all meshes in `MeshToolkit`, the `Mesh.EdgeCount` node only returns unique edges. As a result, it should be expected that the number of edges will not be triple the number of triangles in the mesh. This assumption can be used to verify that the mesh does not contain any unwelded faces (can occur in imported meshes). 

In the example below, `Mesh.Cone` and `Number.Slider` are used to create a cone, which is then used as input to count the edges. Both `Mesh.Edges` and `Mesh.Triangles` can be used to preview the structure and grid of a mesh in preview, `Mesh.Edges` showing better performance for complex and heavy meshes. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)