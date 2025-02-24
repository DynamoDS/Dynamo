## In-Depth
`TSplineVertex.IsTPoint` returns whether a vertex is a T-point. T-points are vertices at the end of partial rows of control points. 

In the example below, `TSplineSurface.SubdivideFaces` is used on a T-Spline box primitive to exemplify one of the multiple ways of adding T-Points to a surface. `TSplineVertex.IsTPoint` node is used to confirm that a vertex at an index is a T-Point. To better visualize the position of T-Points, `TSplineVertex.UVNFrame` and `TSplineUVNFrame.Position` nodes are used. 



## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)