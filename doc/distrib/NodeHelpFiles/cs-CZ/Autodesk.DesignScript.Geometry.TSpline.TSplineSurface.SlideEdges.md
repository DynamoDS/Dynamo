## In Depth
In the example below, a simple T-Spline box surface is created and one of its edges is selected using the `TSplineTopology.EdgeByIndex` node. For a better understanding of the position of the chosen vertex, it is visualized with the help of `TSplineEdge.UVNFrame` and `TSplineUVNFrame.Position` nodes. The chosen edge is passed as input for the `TSplineSurface.SlideEdges` node, along with the surface that it belongs to. The `amount` input determines how much the edge slides towards its neighboring edges, expressed as percentage. The `roundness` input controls the flatness or roundness of the bevel. The effect of the roundness is better understood in box mode. The result of the sliding operation is then translated to the side for preview.  

___
## Example File

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)