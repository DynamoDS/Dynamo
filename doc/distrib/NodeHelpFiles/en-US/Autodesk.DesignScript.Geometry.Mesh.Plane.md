## In-Depth
`Mesh.Plane` creates a flat rectangular mesh using the input point as the origin, given width in x, y directions and given x, y divisions. If either x or y number of subdivisions is set to 0, the default value for either input will result as 5.

In the example below, `Mesh.Plane` is used to create a flat mesh with 4 grid lines along the X-axis and 8 grid lines along the Y-axis. `Mesh.Triangles` node is used to visualize the distribution of mesh triangles.

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.Plane_img.jpg)