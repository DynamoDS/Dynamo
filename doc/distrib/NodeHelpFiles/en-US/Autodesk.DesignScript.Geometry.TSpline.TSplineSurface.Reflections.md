## In Depth
In the example below, a T-Spline surface with added reflections is investigated using the `TSplineSurface.Reflections` node, returning a list of all reflections applied to the surface. The result is a list of two reflections. The same surface is then passed through a `TSplineSurface.RemoveReflections` node and inspected again. This time, the `TSplineSurface.Reflections` node returns an error, due to the fact that the Reflections have been removed. 
___
## Example File

![TSplineSurface.Reflections](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Reflections_img.jpg)