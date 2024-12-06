## In Depth
In the example below, a vertex of a T-Spline surface is collected using the `TSplineTopology.VertexByIndex` node. The vertex is then used as input for the `TSplineSurface.MoveVertices` node. The vertex is moved in a direction specified by the `vector` input. The `onSurface` either considers the surface for movement when set to `True`, or the movement of the control points when set to `False`.
___
## Example File

![TSplineSurface.MoveVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MoveVertices_img.jpg)