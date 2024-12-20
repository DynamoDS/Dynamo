## In-Depth
`Mesh.ByVerticesIndices` takes a list of `Points`, representing the `vertices` of the mesh triangles, and a list of `indices`, representing how the mesh is stitched together, and creates a new mesh. The `vertices` input should be a flat list of unique vertices in the mesh. The `indices` input should be a flat list of integers. Each set of three integers designates a triangle in the mesh. The integers specify the index of the vertex in the vertices list. The indices input should be 0-indexed, with the first point of the vertices list having the index 0. 

In the example below, a `Mesh.ByVerticesIndices` node is used to create a mesh using a list of nine `vertices` and a list of 36 `indices`, specifying the vertex combination for each of the 12 triangles of the mesh.

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)