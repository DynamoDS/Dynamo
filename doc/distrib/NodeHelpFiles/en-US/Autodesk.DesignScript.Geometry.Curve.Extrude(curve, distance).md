## In Depth
`Curve.Extrude (curve, distance)` extrudes an input curve using an input number to determine the distance of the extrusion. The direction of the normal vector along the curve is used for the extrusion direction. 

In the example below, we first create a NurbsCurve by using a `NurbsCurve.ByControlPoints` node, with a set of randomly generated points as the input. Then, we use a `Curve.Extrude` node to extrude the curve. A number slider is used as the `distance` input in the `Curve.Extrude` node.
___
## Example File

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)