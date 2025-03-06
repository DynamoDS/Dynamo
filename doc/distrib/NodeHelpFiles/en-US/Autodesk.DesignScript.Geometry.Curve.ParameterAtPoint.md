## In Depth
Parameter at Point will return the parameter value of a specified point along a curve. If the input point is not on the curve, Parameter At Point will return the parameter of the point on the curve closes to the input point. In the example below, we first create a Nurbs Curve using a ByControlPoints node, with a set of randomly generated points as the input. An extra single point is created with a Code Block to specify the x and y coordinates. The ParameterAtPoint node returns the parameter along the curve at the point that is closest to the input point.
___
## Example File

![ParameterAtPoint](./Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint_img.jpg)

