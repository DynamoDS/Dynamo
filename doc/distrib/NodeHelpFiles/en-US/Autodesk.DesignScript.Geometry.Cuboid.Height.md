## In Depth
`Cuboid.Height` returns the height of the input cuboid. Note that if the cuboid has been transformed to a different coordinate system with a scale factor, this will return the original dimensions of the cuboid, not the world space dimensions. In other words, if you create a cuboid with a width (X-axis) of 10 and transform it to a CoordinateSystem with 2 times scaling in X, the width will still be 10. 

In the example below, we generate a cuboid by corners, and then use a `Cuboid.Height` node to find its height. 

___
## Example File

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

