## In Depth
The `TSplineSurface.MakeUniform` node spaces out the knots intervals of a surface evenly. This can come handy when the surface has bunching after adding control points.

In the example below, a T-Spline surface is created from a NURBS surface using the curvature subdivision method. The T-Spline surface is compared before and after passing the `TSplineSurface.MakeUniform` node. 

 
___
## Example File

![TSplineSurface.MakeUniform](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MakeUniform_img.jpg)