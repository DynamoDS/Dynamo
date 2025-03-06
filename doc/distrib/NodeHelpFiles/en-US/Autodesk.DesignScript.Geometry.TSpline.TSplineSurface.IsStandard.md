## In Depth
A T-Spline surface is standard when all T-points are separated from star points by at least two isocurves. Standardization is necessary for converting a T-Spline surface into a NURBS surface.

In the example below, a T-Spline surface generated through `TSplineSurface.ByBoxLengths` has one of its faces subdivided. `TSplineSurface.IsStandard` is used to check if the surface is standard, but it yields a negative result.
`TSplineSurface.Standardize` is then employed to standardize the surface. New control points are introduced without altering the shape of the surface. The resulting surface is checked using `TSplineSurface.IsStandard`, which confirms that it is now standard.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Example File

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)