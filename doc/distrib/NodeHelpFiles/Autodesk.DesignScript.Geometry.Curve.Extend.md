## In Depth
Extend will extend a input curve by a given input distance. The pickSide input takes the start or end point of the curve as input, and determines which end of the curve to extend. In the example below, we first create a Nurbs Curve using a ByControlPoints node, with a set of randomly generated points as the input. We use the query node Curve.EndPoint to find the end point of the curve, to use as the 'pickSide' input. A number slider allows us to control the distance of the extension.
___
## Example File

![Extend](./Autodesk.DesignScript.Geometry.Curve.Extend_img.jpg)

