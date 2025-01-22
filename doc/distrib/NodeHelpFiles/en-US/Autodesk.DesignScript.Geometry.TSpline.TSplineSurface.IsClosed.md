## In Depth
A closed surface is one that forms a complete shape with no openings or boundaries.
In the example below, a T-Spline sphere generated through `TSplineSurface.BySphereCenterPointRadius` is inspected using `TSplineSurface.IsClosed` to check if it's open, which returns a negative result. That's because T-Spline spheres, although they appear closed, are actually open at poles where multiple edges and vertices stack in one point.

The gaps in the T-Spline sphere are then filled using the `TSplineSurface.FillHole` node, which results in a slight deformation where the surface was filled. When it's checked again through the `TSplineSurface.IsClosed` node, it now yields a positive result, meaning that it is closed.
___
## Example File

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)