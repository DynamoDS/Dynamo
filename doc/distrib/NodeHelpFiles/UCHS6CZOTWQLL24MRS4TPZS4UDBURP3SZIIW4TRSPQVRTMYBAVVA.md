<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
In the example below, the node `TSplineSurface.UncreaseVertices` is used on corner vertices of a plane primitive. By default, these vertices are creased at the moment when the surface is created. The vertices are identified with the help of `TSplineVertex.UVNFrame` and `TSplineUVNFrame.Poision` nodes, with `Show Labels` option activated. The corner vertices are then selected using the `TSplineTopology.VertexByIndex` node and uncreased. The effect of this action can be previewed if the shape is in smooth mode preview.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices_img.jpg)
