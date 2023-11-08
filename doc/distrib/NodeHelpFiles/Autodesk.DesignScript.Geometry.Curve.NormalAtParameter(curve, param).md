## In Depth
`Curve.NormalAtParameter (curve, param)` returns a vector aligned with the normal direction at the specified parameter of a curve. The parameterization of a curve is measured in the range from 0 to 1, with 0 representing the start of the curve and 1 representing the end of the curve. 

In the example below, we first create a NurbsCurve using a `NurbsCurve.ByControlPoints` node, with a set of randomly generated points as the input. A number slider set to the range 0 to 1 is used to control the `parameter` input for a `Curve.NormalAtParameter` node.
___
## Example File

![Curve.NormalAtParameter(curve, param](./Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve,%20param)_img.jpg)