## In Depth
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` first splits a curve at points determined by an input list of parameters. It then returns either the odd numbered segments or the even numbered segments, as determined by the Boolean value of the `discardEvenSegments` input. 

In the example below, we first create a NurbsCurve using a `NurbsCurve.ByControlPoints` node, with a set of randomly generated points as the input. A `code block` is used to create a range of numbers between 0 and 1, stepping by 0.1. Using this as the input parameters for a `Curve.TrimSegmentsByParameter` node results in a list of curves that are effectively a dashed-line version of the original curve.
___
## Example File

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve,%20parameters,%20discardEvenSegments)_img.jpg)