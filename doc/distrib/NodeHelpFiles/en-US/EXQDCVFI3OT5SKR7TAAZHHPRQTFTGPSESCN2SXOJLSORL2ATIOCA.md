<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## In Depth
Curve.ExtrudeAsSolid (direction, distance) extrudes an input closed, planar curve using an input vector to determine the direction of the extrusion. A separate `distance` input is used for the extrusion distance. This node caps the ends of the extrusion to create a solid. 

In the example below, we first create a NurbsCurve using a `NurbsCurve.ByPoints` node, with a set of randomly generated points as the input. A `code block` is used to specify the X, Y, and Z components of a `Vector.ByCoordinates` node. This vector is then used as the direction input in a `Curve.ExtrudeAsSolid` node while a number slider is used to control the `distance` input.
___
## Example File

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)