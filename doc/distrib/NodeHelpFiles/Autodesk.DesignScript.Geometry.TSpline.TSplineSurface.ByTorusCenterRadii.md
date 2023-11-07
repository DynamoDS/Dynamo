## In-Depth
In the example below, a T-Spline torus surface is created around a given `center`. The minor and major radii of the shape are set by inputs `innerRadius` and `outerRadius`. The values for `innerRadiusSpans` and `outerRadiusSpans` control the definition of the surface along the two directions. The initial symmetry of the shape is specified by the `symmetry` input. If Axial symmetry applied to the shape is active for the X or Y axis, the value of `outerRadiusSpans` of the torus must be a multiple of 4. Radial symmetry has no such requirement. Finally, the `inSmoothMode` input is used to switch between smooth and box mode preview of the T-Spline surface.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCenterRadii_img.jpg)


