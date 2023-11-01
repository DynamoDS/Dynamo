## In-Depth
Note that in a T-Spline surface topology, indices of `Face`, `Edge`, and `Vertex` do not necessarily coincide with the sequence number of the item in the list. Use the node `TSplineSurface.CompressIndices` to address this issue.

In the example below `TSplineTopology.DecomposedEdges` is used to retrieve the border edges of a T-Spline surface and `TSplineEdge.Index` node is then used to obtain the indices of the provided edges.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)