## In Depth

In the example below, a NURBS surface of degree 3 is converted into a T-Spline surface using `TSplineSurface.ByNurbsSurfaceUniform` node. Input NURBS surface is rebuilt with uniform knots placed at equal parametric or arc length intervals depending on corresponding `uUseArcLen` and `vUseArcLen` inputs, and approximated by degree 3 NURBS surface. Output T-Spline is divided by given `uSpan` and `vSpan` counts in U and V directions. 
___
## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByNurbsSurfaceUniform_img.jpg)
