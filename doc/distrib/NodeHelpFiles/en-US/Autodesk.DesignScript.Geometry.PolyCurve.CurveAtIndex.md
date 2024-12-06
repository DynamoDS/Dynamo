## In Depth
Curve At Index will return the curve segment at the input index of a given polycurve. If the number of curves in the polycurve is less than the give index, CurveAtIndex will return null. The endOrStart input accepts a boolean value of true or false. If true, CurveAtIndex will begin counting at the first segment of the PolyCurve. If false, it will count backwards from the last segment. In the example below, we generate a set of random points, and then use PolyCurve By Points to create an open PolyCurve. We can then use CurveAtIndex to extract specfic segments from the PolyCurve.
___
## Example File

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

