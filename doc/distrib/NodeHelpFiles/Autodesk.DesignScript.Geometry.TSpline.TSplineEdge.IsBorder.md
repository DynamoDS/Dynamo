## In-Depth
`TSplineEdge.IsBorder` returns `True` if the input T-Spline edge is a Border.

In the example below, the edges of two T-Spline surfaces are investigated. The surfaces are a cylinder and its thickened version. To select all edges, `TSplineTopology.EdgeByIndex` nodes are used in both cases, with the indices input - a range of integers spanning from 0 to n, where n is the number of edges provided by the `TSplineTopology.EdgesCount`. This is an alternative to directly selecting edges using `TSplineTopology.DecomposedEdges`. `TSplineSurface.CompressIndices` is additionally used in the case of a thickened cylinder to reorder the edge indices. 
`TSplineEdge.IsBorder` node is used to check which of the edges are border edges. The position of the border edges of the flat cylinder are highlighted with the help of `TSplineEdge.UVNFrame` and `TSplineUVNFrame.Position` nodes. The thickened cylinder has no border edges.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)