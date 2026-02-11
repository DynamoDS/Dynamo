<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## In Depth
`TSplineTopology.BorderVertices` returns a list of border vertices contained in a T-Spline surface.

In the example below, two T-Spline Surfaces are created through `TSplineSurface.ByCylinderPointsRadius`. One is an open surface while the other is thickened using `TSplineSurface.Thicken`, which turns it into a closed surface. When both are examined with the `TSplineTopology.BorderVertices` node, the first one returns a list of border vertices while the second one returns an empty list. That's because since the surface is enclosed, there are no border vertices.
___
## Example File

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)