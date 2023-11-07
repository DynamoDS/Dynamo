## In-Depth
Similar to `TSplineSurface.UnweldEdges`, this node performs the unweld operation on a set of vertices. As a result, all edges joining at the selected vertex are unwelded. Unlike the Uncrease operation that creates a sharp transition around the vertex while maintaining the connection, Unweld creates a discontinuity. 

In the example below, one of the selected vertices of a T-Spline plane is unwelded with `TSplineSurface.UnweldVertices` node. A discontinuity is introduced along the edges surrounding the chosen vertex, which is illustrated by pulling a vertex up with `TSplineSurface.MoveVertices` node.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices_img.jpg)
