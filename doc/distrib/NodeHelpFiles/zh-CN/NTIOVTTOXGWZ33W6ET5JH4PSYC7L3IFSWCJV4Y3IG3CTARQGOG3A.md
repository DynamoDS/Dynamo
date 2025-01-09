<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## 详细
`TSplineSurface.BridgeEdgesToEdges` 连接两组边，这些边来自同一个曲面或两个不同的曲面。该节点需要如下所述的输入。前三个输入足以生成桥接，其余输入是可选的。生成的曲面是第一组边所属曲面的子曲面。

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: 选定的同一个 T-Spline 曲面或其他 T-Spline 曲面中的边。边数必须匹配，或是桥接另一侧边数的倍数。
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


在下面的示例中，创建两个 T-Spline 平面，使用 `TSplineSurface.DeleteEdges` 节点删除每个 T-Spline 平面中心中的面。使用 `TSplineTopology.VertexByIndex` 节点收集已删除面周围的边。要创建桥接，两组边以及其中一个曲面用作 `TSplineSurface.BrideEdgesToEdges` 节点的输入。这将创建桥接。通过编辑 `spansCounts` 输入，将更多跨度添加到桥接。当曲线用作 `followCurves` 的输入时，桥接遵循所提供曲线的方向。`keepSubdCreases`、`frameRotations`、`firstAlignVertices` 和 `secondAlignVertices` 输入说明如何微调桥接的形状。

## 示例文件

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

