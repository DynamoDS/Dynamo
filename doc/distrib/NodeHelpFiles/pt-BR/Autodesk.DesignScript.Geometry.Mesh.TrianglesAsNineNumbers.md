## In-Depth
`Mesh.TrainglesAsNineNumbers` determines the X, Y and Z coordinates of the vertices composing each triangle in a provided mesh, resulting in nine numbers per triangle. This node can be useful for querying, reconstructing or converting the original mesh. 

In the example below, `File Path` and `Mesh.ImportFile` are used to import a mesh. Then `Mesh.TrianglesAsNineNumbers` is used to obtain the coordinates of the vertices of each triangle. This list is then subdivided into threes using the `List.Chop` with `lengths` input set to 3. `List.GetItemAtIndex` is then used to get each X, Y and Z coordinate and rebuild the vertices using `Point.ByCoordinates`. The list of points is further divided into threes (3 points for each triangle) and is used as input for `Polygon.ByPoints`. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)