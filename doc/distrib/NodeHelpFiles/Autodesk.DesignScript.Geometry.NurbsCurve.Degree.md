## In Depth
Degree will return the degree of a NURBS curve. In the example, we first generate a number of random points, and then use NurbsCurve ByPoints to create a curve interpolated through the points. We can use Degree to then get the degree of the curve. Since we created the curve without specifying the degree, it used a default degree of three. (A polygonal curve of straight lines has a degree of one, while the most common degree for non-straight-segmented curves is 3)
___
## Example File

![Degree](./Autodesk.DesignScript.Geometry.NurbsCurve.Degree_img.jpg)

