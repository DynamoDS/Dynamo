## In Depth
`TSplineSurface.FlattenVertices(vertices, parallelPlane)` alters the positions of control points for a specified set of vertices by aligning them with a `parallelPlane` provided as input. 

In example below, vertices of a T-Spline plane surface are displaced using `TsplineTopology.VertexByIndex` and `TSplineSurface.MoveVertices` nodes. The surface is then translated to the side for a better preview and used as input for `TSplineSurface.FlattenVertices(vertices, parallelPlane)` node. The result is new surface with selected vertices lying flat on the provided plane.
___
## Example File

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)