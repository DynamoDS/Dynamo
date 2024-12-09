<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections` removes the Reflections from the input `tSplineSurface`. Removing reflections does not modify the shape but breaks the dependency between the reflected parts of the geometry, allowing you to edit them indepentently.

In the example below, a T-Spline surface is first created by applying Axial and Radial reflections. The surface is then passed into the `TSplineSurface.RemoveReflections` node, thus removing the reflections. To illustrate how this affects later alterations, one of the vertices is moved using a `TSplineSurface.MoveVertex` node. Due to the reflections being removed from the surface, only one vertex is modified. 

## Example File

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)