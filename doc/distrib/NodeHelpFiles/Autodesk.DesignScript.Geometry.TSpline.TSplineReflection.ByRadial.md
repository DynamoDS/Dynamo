## In-Depth
`TSplineReflection.ByRadial` returns a `TSplineReflection` object that can be used as input for the `TSplineSurface.AddReflections` node. The node takes a plane as input, and the normal of the plane acts as the axis for rotating the geometry. Much like TSplineInitialSymmetry, TSplineReflection, once established at the creation of the TSplineSurface, influences all subsequent operations and alterations.

In the example below, `TSplineReflection.ByRadial` is used to define the reflection of a T-Spline surface. The `segmentsCount` and `segmentAngle` inputs are used to control the way that geometry is reflected around the normal of a given plane. The output of the node is then used as input for the `TSplineSurface.AddReflections` node to create a new T-Spline surface. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)