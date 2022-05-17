## In Depth
Plane At Parameter will return a plane aligned with a curve at a specified parameter. The normal vector of the resulting plane will correspond to the tangent of the curve. The parameterization of a curve is measured in the range from zero to one, with zero representing the start of the curve and one representing the end of the curve. In the example below, we first create a Nurbs Curve using a ByControlPoints node, with a set of randomly generated points as the input. A number slider set to the range 0 to 1 is used to control the parameter input for a PlaneAtParameter node.
___
## Example File

![PlaneAtParameter](./Autodesk.DesignScript.Geometry.Curve.PlaneAtParameter_img.jpg)

