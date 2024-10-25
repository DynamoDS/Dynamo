## In Depth
`TSplineTopology.BorderEdges` returns a list of border edges contained in T-Spline surface.

In the example below, two T-Spline Surfaces are created through `TSplineSurface.ByCylinderPointsRadius`; one is an open surface while the other is thickened using `TSplineSurface.Thicken`, which turns it into a closed surface. When both are examined with the `TSplineTopology.BorderEdges` node, the first one returns a list of border edges while the second one returns an empty list. That's because since the surface is enclosed, there are no border edges.
___
## Example File

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)