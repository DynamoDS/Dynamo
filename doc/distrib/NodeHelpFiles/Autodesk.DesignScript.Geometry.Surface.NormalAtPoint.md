## In Depth
Normal At Point finds the Normal vector of the surface at the input point on a surface. If the input point is not on the surface, this node will find the point on the surface that is nearest to the input point. In the example below, we first create a surface by using a BySweep2Rails. We then use a Code Block to specify a point to find the normal at. The point is not on the surface, so the node uses the closest point on the surface as the position to find the normal at.
___
## Example File

![NormalAtPoint](./Autodesk.DesignScript.Geometry.Surface.NormalAtPoint_img.jpg)

