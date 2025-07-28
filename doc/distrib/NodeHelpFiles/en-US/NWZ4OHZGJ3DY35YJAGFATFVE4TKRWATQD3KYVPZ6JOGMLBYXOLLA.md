<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## In Depth
`Curve.ExtrudeAsSolid (curve, distance)` extrudes an input closed, planar curve using an input number to determine the distance of the extrusion. The direction of the extrusion is determined by the normal vector of the plane that the curve lies in. This node caps the ends of the extrusion to create a solid. 

In the example below, we first create a NurbsCurve by using a `NurbsCurve.ByPoints` node, with a set of randomly generated points as the input. Then, a `Curve.ExtrudeAsSolid` node is used to extrude the curve as a solid. A number slider is used as the `distance` input in the `Curve.ExtrudeAsSolid` node.
___
## Example File

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)