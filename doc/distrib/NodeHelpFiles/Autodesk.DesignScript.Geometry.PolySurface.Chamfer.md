## In Depth
Chamfer will return a new solid with chamfered edges. The edges input specifies which edges to chamfer, while the offset input determines the extent of the chamfer. In the example below, we start with a cube using the default inputs. To get the appropriate edges of the cube, we first explode the cube to get the faces as a list of surfaces. We then use a Face.Edges node to extract the edges of the cube. We extract the first edge of each face with GetItemAtIndex. A number slider controls the offset distance for the chamfer.
___
## Example File



