<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## 详细
`TSplineSurface.BridgeEdgesToFaces` 将一组边与一组面连接，这些边和面来自同一个曲面或来自两个不同的曲面。组成面的边需要在数量上匹配，或是桥接另一侧边的倍数。该节点需要以下所述的输入。前三个输入足以生成桥接，其余输入是可选的。生成的曲面是第一组边所属曲面的子曲面。

- `TSplineSurface`: the surface to bridge
- `firstGroup`: 选定 TSplineSurface 中的边
- `secondGroup`: 选定的同一个 T-Spline 曲面或其他 T-Spline 曲面中的面。
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


在下面的示例中，创建两个 T-Spline 平面，使用 `TSplineTopology.VertexByIndex` 和 `TSplineTopology.FaceByIndex` 节点收集边集和面集。要创建桥接，面和边以及其中一个曲面用作 `TSplineSurface.BrideEdgesToFaces` 节点的输入。这将创建桥接。通过编辑 `spansCounts` 输入，将更多跨度添加到桥接。当曲线用作 `followCurves` 的输入时，桥接遵循所提供曲线的方向。`keepSubdCreases`、`frameRotations`、`firstAlignVertices` 和 `secondAlignVertices` 输入说明如何微调桥接的形状。

## 示例文件

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

