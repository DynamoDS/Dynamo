<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## In Depth
`TSplineSurface.BridgeEdgesToFaces` connects two sets of faces, either from the same surface or from two different surfaces. The node requires the inputs described below. The first three inputs are enough to generate the bridge, the rest of the inputs being optional. The resulting surface is a child of the surface that the first group of edges belongs to.

In the example below, a torus surface is created using `TSplineSurface.ByTorusCenterRadii`. Two of its faces are selected and used as input for the `TSplineSurface.BridgeFacesToFaces` node, along with the torus surface. The rest of the inputs demonstrate how the bridge can be further adjusted: 
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases.
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge. The torus surface has no creased edges, so this input has no effect on the shape. 
- `firstAlignVertices`(optional) and `secondAlignVertices`: by specifying a shifted pair of vertices, the bridge acquires a light rotation.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## Example File

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
