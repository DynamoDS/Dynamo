## In Depth
`TSplineSurface.BridgeEdgesToEdges` connects two sets of edges either from the same surface or from two different surfaces. The node requires the inputs rescribed below. The first three inputs are enough to generate the bridge, the rest of the inputs being optional. The resulting surface is a child of the surface that the first group of edges belongs to.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: edges from either the same T-Spline surface selected, or from a different one. The number of edges must match in number, or be a multiple of the number of edges on the other side of the bridge.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional)reverses the direction of vertices to align


In the example below, two T-Spline planes are created and a face in the center of each is deleted using `TSplineSurface.DeleteEdges` node. The edges around the deleted face are collected using `TSplineTopology.VertexByIndex` node. To create a bridge, two groups of edges are used as input for the `TSplineSurface.BrideEdgesToEdges` node, along with one of the surfaces. This creates the bridge. More spans are added to the bridge by editing the `spansCounts` input. When a curve is used as input for `followCurves`, the bridge follows the direction of the provided curve. `keepSubdCreases`,`frameRotations`, `firstAlignVertices` and `secondAlignVertices` inputs demonstrate how the shape of the bridge can be fine-tuned. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges_img.gif)

