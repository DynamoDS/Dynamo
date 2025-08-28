## In Depth
Fillet will return a new solid with rounded edges. The edges input specifies which edges to fillet, while the offset input determines the radius of the fillet. In the example below, we start with a cube using the default inputs. To get the appropriate edges of the cube, we first explode the cube to get the faces as a list of surfaces. We then use a Face.Edges node to extract the edges of the cube. We extract the first edge of each face with GetItemAtIndex. A number slider controls the radius for each fillet.
___
## Example File

![Fillet](./Autodesk.DesignScript.Geometry.Solid.Fillet_img.jpg)

