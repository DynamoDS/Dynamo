## In Depth
Split By Points will split an input curve at specified points, and will return a list of resulting segments. If the specified points are not on the curve, this node will find the points along the curve that are closest to the input points and split the curve at those resulting points. In the example below, we first create a Nurbs Curve using a ByPoints node, with a set of randomly generated points as the input. The same set of points is used as the list of points in a SplitByPoints node. The result is a list of curve segments between the generated points.
___
## Example File

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

