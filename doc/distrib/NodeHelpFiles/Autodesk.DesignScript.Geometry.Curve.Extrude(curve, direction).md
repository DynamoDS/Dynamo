## In Depth
`Curve.Extrude (curve, direction)` extrudes an input curve using an input vector to determine the direction of the extrusion. The length of the vector is used for the extrusion distance. 

In the example below, we first create a NurbsCurve using a `NurbsCurve.ByControlPoints` node, with a set of randomly generated points as the input. A code block is used to specify the X, Y, and Z components of a `Vector.ByCoordinates` node. This vector is then used as the `direction` input in a `Curve.Extrude` node.
___
## Example File

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)