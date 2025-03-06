## In Depth
`PolyCurve.Heal` takes a self-intersecting PolyCurve and returns a new PolyCurve that does not self-intersect. The input PolyCurve may not have more than 3 self-intersections. In other words, if any single segment of the PolyCurve meets or intersects more than 2 other segments, the heal won’t work. Input a `trimLength` greater than 0, and end segments longer than the `trimLength` will not be trimmed. 

In the example below, a self-intersecting PolyCurve is healed using `PolyCurve.Heal`. 
___
## Example File

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)