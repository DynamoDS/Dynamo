## In Depth
`Surface.ToNurbsSurface` takes a surface as input and returns a NurbsSurface that approximates the input surface. The `limitSurface` input determines if the surface should be restored to its original parameter range before conversion, for example, when the parameter range of a surface is limited is after a Trim operation.

In the example below, we create a surface using a `Surface.ByPatch` node with a closed NurbsCurve as an input. Note that when we use this surface as the input for a `Surface.ToNurbsSurface` node, the result is an untrimmed NurbsSurface with four sides.


___
## Example File

![Surface.ToNurbsSurface](./Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface,%20limitSurface)_img.jpg)