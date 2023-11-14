## In Depth
`TSplineSurface.Standardize` node is used to standardize a T-Spline surface. 
Standardizing means preparing a T-Spline surface for NURBS conversion and implies extending all T-Points until they are separated from star points by at least two isocurves. Standardizing doesn't change the shape of the surface but may add control points to meet the geometry requirements necessary to make the surface NURBS-compatible.

In the example below, a T-Spline surface generated through `TSplineSurface.ByBoxLengths` has one of its faces subdivided.
`TSplineSurface.IsStandard` node is used to check if the surface is standard, but it yields a negative result.
`TSplineSurface.Standardize` is then employed to standardize the surface. The resulting surface is checked using `TSplineSurface.IsStandard`, which confirms that it is now standard.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Example File

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)