<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## In Depth
`Curve.SweepAsSurface` will create a surface by sweeping an input curve along a specfied path. In the example below, we create a curve to sweep by using a Code Block to create three points of an `Arc.ByThreePoints` node. A path curve is created as a simple line along the x-axis. `Curve.SweepAsSurface` moves the profile curve along the path curve creating a surface. The `cutEndOff` parameter is a boolean that controls the end treatment of the swept surface. When set to `true`, the ends of the surface are cut perpendicular (normal) to the path curve, producing clean, flat terminations. When set to `false` (the default), the surface ends follow the natural shape of the profile curve without any trimming, which may result in angled or uneven ends depending on the path curvature.
___
## Example File

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

