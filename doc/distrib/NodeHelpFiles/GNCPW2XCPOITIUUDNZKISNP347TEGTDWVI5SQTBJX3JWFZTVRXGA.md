## In Depth
In the example below, a planar TSpline surface with extruded, subdivided and pulled vertices and faces is inspected with the `TSplineTopology.DecomposedVertices` node, which returns a list of the following types of vertices contained in the TSpline surface:

- `all`: list of all vertices
- `regular`: list of regular vertices
- `tPoints`: list of TPoint vertices
- `starPoints`: list of StarPoint vertices
- `nonManifold`: list of NonManifold vertices
- `border`: list of border vertices
- `inner`: list of inner vertices

The nodes `TSplineVertex.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the different types of vertices in the surface.

___
## Example File

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)