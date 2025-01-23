## In-Depth
This node returns a TSplineUVNFrame object that can be useful for visualizing the vertex position and orientation, as well as using the U,V or N vectors to further manipulate the T-Spline surface.

In the example below, `TSplineVertex.UVNFrame` node is used to obtain the UVN Frame of the selected vertex. The UVN frame is then used to return the normal of the vertex. Lastly, the normal direction is used to move the vertex using the `TSplineSurface.MoveVertices` node.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)