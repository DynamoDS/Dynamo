## In Depth
`PolyCurve.Points` returns the start point for the first component curve, and end points for all other component curves. It does not return duplicate points for closed PolyCurves.

In the example below, a `Polygon.RegularPolygon` is exploded into a curve list and is then rejoined into a PolyCurve. The PolyCurve’s points are then returned using `PolyCurve.Points`.
___
## Example File

![PolyCurve.Points](./Autodesk.DesignScript.Geometry.PolyCurve.Points_img.jpg)