## In-Depth
`TSplineFace.Index` returns the index of the face on the T-Spline surface. Note that in a T-Spline surface topology, indices of Face, Edge, and Vertex do not necessarily coincide with the sequence number of the item in the list. Use the node `TSplineSurface.CompressIndices` to address this issue.

In the example below, `TSplineFace.Index` is used to show the indices of all regular faces of a T-Spline Surface.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Index_img.jpg)