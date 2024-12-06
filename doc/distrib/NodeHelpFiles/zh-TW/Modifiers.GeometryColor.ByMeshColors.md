## In-Depth
`GeometryColor.ByMeshColor` returns a GeometryColor object which is a mesh colored following the given color list. There are a couple of ways to use this node: 

- if one color is provided, the entire mesh is colored with one given color;
- if the number of colors matches the number of triangles, each triangle is colored the corresponding color from the list;
- if the number of colors matches the number of unique vertices, the color of each triangle in the mesh color interpolates between the color values at each vertex;
- if the number of colors equals the number of non-unique vertices, the color of each triangle interpolates between the color values across a face but may not blend between faces. 

## Example

In the example below, a mesh is color-coded based on the elevation of its vertices. First, `Mesh.Vertices` is used to get unique mesh vertices which are then analyzed and the elevation of each vertex point is obtained using `Point.Z` node. Second, `Map.RemapRange` is used to map the values to a new range of 0 to 1 by scaling each value proportionally. Finally, `Color Range` is used to generate a list of colors corresponding with the mapped values. Use this list of colors as the `colors` input of the `GeometryColor.ByMeshColors` node. The result is a color-coded mesh where the color of each triangle interpolates between vertex colors resulting in a gradient.

## Example File

![Example](./Modifiers.GeometryColor.ByMeshColors_img.jpg)