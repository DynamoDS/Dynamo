## In-Depth
`TSplineVertex.Index` returns the index number of the chosen vertex on the T-Spline Surface. Note that in a T-Spline surface topology, indices of Face, Edge, and Vertex do not necessarily coincide with the sequence number of the item in the list. Use the node `TSplineSurface.CompressIndices` to address this issue.

In the example below, `TSplineTopology.StarPointVertices` is used on a T-Spline primitive in the shape of a box. `TSplineVertex.Index` is then used to query the indices of start-point vertices and `TSplineTopology.VertexByIndex` returns the selected vertices for further editing. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)