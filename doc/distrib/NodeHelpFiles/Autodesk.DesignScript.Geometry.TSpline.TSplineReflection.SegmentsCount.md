## In-Depth
`TSplineReflection.SegmentsCount` returns the number of segments of a Radial Reflection. If the type of TSplineReflection is Axial, the node returns a value of 0. 

In the example below, a T-Spline surface is created with added Reflections. Later in the graph, the surface is interrogated with `TSplineSurface.Reflections` node. The result (a Reflection) is then used as input for the `TSplineReflection.SegmentsCount` to return the number of segments of a Radial Reflection that was used to create the T-Spline surface.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount_img.jpg)