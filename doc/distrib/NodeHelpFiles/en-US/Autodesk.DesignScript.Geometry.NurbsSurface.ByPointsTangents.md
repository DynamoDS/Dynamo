## In Depth
`NurbsSurface.ByPointsTangents` creates a smooth surface that passes through each of a grid (list of lists) of points. Specify tangent vectors to control the surface direction at the edges. The number of tangents must match the number of points in the corresponding direction (U - number of lists, V - number of points in each list).

In the example below, a NurbsSurface is created from given points and U and V tangents.

___
## Example File

![NurbsSurface.ByPointsTangents](./Autodesk.DesignScript.Geometry.NurbsSurface.ByPointsTangents_img.jpg)