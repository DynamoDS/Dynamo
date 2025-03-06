## In-Depth
`Mesh.ByGeometry` takes Dynamo geometry objects (Surfaces or Solids) as input and converts them into a mesh. Points and Curves have no mesh representations so they are not valid inputs. The resolution of the mesh produced in the conversion is controlled by the two inputs - `tolerance` and `maxGridLines`. The `tolerance` sets the acceptable deviation of the mesh from the original geometry and is subject to the size of the mesh. If the value of `tolerance` is set to -1, Dynamo chooses a sensible tolerance. `maxGridLines` input sets the maximum number of grid lines in the U or V direction. A higher number of grid lines helps increase the smoothness of the tesselation. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)