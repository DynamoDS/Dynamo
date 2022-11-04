## In Depth
Containment Test returns a boolean value depending on whether a given point is contained inside a given polygon. The polygon must be planar and non-self-intersecting in order for this to work. In the example below, we create a polygon using a series of points created By Cylindrical Coordinates. Leaving the elevation constant, and sorting the angles ensures a planar and non-self-intersecting polygon. We then create a point to test, and use ContainmentTest to see if the point is inside or outside the polygon.
___
## Example File

![ContainmentTest](./Autodesk.DesignScript.Geometry.Polygon.ContainmentTest_img.jpg)

