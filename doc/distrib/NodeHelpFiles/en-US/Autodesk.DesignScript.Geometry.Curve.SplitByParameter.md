## In Depth
`Curve.SplitByParameter (curve, parameters)` takes a curve and a list of parameters as inputs. It splits the curve at the specified parameters and returns a list of resulting curves. 

In the example below, we first create a NurbsCurve using a `NurbsCurve.ByControlPoints` node, with a set of randomly generated points as the input. A code block is used to create a series of numbers between 0 and 1 to use as the list of parameters at which the curve is split.

___
## Example File

![SplitByParameter](./Autodesk.DesignScript.Geometry.Curve.SplitByParameter_img.jpg)

