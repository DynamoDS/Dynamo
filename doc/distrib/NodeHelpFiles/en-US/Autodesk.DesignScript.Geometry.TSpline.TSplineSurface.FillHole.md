## In Depth
In the example below, the gaps in a T-Spline Cylindric Surface are filled using the `TSplineSurface.FillHole` node, which requires the following inputs:
- `edges`: a number of border edges picked from T-Spline surface to fill
- `fillMethod`: a numerical value from 0-3 which indicates the filling method:
    * 0 fills the hole with tesselation
    * 1 fills the hole with a single NGon face
    * 2 creates a point in the center of the hole from which triangular faces radiate towards the edges
    * 3 is similar to method 2, with a difference that the center vertices are welded into one vertex instead of just stacked on top.
- `keepSubdCreases`: a boolean value that indicates whether subd-creases are preserved.
___
## Example File

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)