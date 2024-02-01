<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## In Depth
In the example below, a planar T-Spline surface with extruded, subdivided, and pulled vertices and faces is inspected with the `TSplineTopology.DecomposedVertices` node, which returns a list of the following types of vertices contained in the T-Spline surface:

- `all`: list of all vertices
- `regular`: list of regular vertices
- `tPoints`: list of T-Point vertices
- `starPoints`: list of Star Point vertices
- `nonManifold`: list of Non-Manifold vertices
- `border`: list of border vertices
- `inner`: list of inner vertices

The nodes `TSplineVertex.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the different types of vertices of the surface.

___
## Example File

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
