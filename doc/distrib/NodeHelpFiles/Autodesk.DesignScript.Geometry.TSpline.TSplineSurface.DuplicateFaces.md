## In Depth
The node `TSplineSurface.DuplicateFaces` creates a new T-Spline surface made of selected copied faces only. 

In the provided example, a T-Spline surface is created through `TSplineSurface.ByRevolve`, using a NURBS curve as a profile. 
A set of faces on the surface is then selected using the `TSplineTopology.FaceByIndex`. These faces are duplicated using `TSplineSurface.DuplicateFaces` and the resulting surface is shifted to the side for a better visualization.
___
## Example File

![TSplineSurface.DuplicateFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces_img.jpg)