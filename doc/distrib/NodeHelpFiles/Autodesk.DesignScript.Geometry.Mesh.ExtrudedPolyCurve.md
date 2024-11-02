## In-Depth
`Mesh.ExtrudePolyCurve` node extrudes a provided `polycurve` by a given distance set by the `height` input and in the specified vector direction. Open polycurves are closed by connecting first point to last. If the initial `polycurve` is planar and not self-intersecting, the resulting mesh has the option of being capped to form a solid mesh.
In the example below, `Mesh.ExtrudePolyCurve` is used to create a closed mesh based on a closed polycurve. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.ExtrudedPolyCurve_img.jpg)