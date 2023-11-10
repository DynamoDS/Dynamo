## In Depth
`TSplineSurface.Standardize` node is used to standardize a T-Spline surface. 
Standardizing a T-Spline surface means extending all T-points within two isocurves of star points until the T-points are separated from star points by at least two isocurves. This process prepares a T-Spline surface for conversion into a NURBS-compatible surface. It doesn't change the shape of the surface but may add control points to meet the geometry requirements necessary for NURBS conversion.

In the example below, a T-Spline surface generated through `TSplineSurface.ByBoxLengths` has one of its faces subdivided.
`TSplineSurface.IsStandard` is used to check if the surface is standard, but it yields a negative result.
`TSplineSurface.Standardize` is then employed to standardize the surface. The resulting surface is checked using `TSplineSurface.IsStandard`, which confirms that it is now standard.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Example File

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)