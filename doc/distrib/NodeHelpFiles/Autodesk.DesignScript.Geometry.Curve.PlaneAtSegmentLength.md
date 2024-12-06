## In Depth
Plane At Segment Length will return a plane aligned with a curve at a point that is a specified distance along the curve, measured from the start point. If the input length is greater than the total length of the curve, this node will use the end point of the curve. The normal vector of the resulting plane will correspond to the tangent of the curve. In the example below, we first create a Nurbs Curve using a ByControlPoints node, with a set of randomly generated points as the input. A number slider is used to control the parameter input for a PlaneAtSegmentLength node.
___
## Example File

![PlaneAtSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength_img.jpg)

