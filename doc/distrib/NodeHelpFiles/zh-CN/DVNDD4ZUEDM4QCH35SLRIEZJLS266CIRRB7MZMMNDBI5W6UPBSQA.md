<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## 详细
`TSplineSurface.BridgeToFacesToEdges` 将一组边与一组面连接，这些边和面来自同一个曲面或来自两个不同的曲面。组成面的边需要在数量上匹配，或是桥接另一侧边的倍数。该节点需要以下所述的输入。前三个输入足以生成桥接，其余输入是可选的。生成的曲面是第一组边所属曲面的子曲面。

- `TSplineSurface`: 要桥接的曲面
- `firstGroup`: 选定 TSplineSurface 中的面
- `secondGroup`: 选定的同一个 T-Spline 曲面或其他 T-Spline 曲面中的边。边数必须匹配，或是桥接另一侧边数的倍数。
- `followCurves`: (可选)供桥接遵循的曲线。在没有此输入的情况下，桥接遵循直线
- `frameRotations`: (可选)连接所选边的桥接拉伸的旋转数。
- `spansCounts`: (可选)连接所选边的桥接拉伸的跨度/分段数。如果跨度数太小，某些选项可能不可用，直到增加跨度数。
- `cleanBorderBridges`:(可选)删除边界桥接之间的桥接以防止锐化
- `keepSubdCreases`: (可选)保留输入拓扑的细分锐化，从而对桥接的起点和终点进行锐化
- `firstAlignVertices` (可选) 和 `secondAlignVertices`: 强制两组顶点之间的对齐，而不是自动选择最接近的顶点对。
- `flipAlignFlags`: (可选)反转要对齐的顶点的方向


在下面的示例中，创建两个 T-Spline 平面，使用 `TSplineTopology.VertexByIndex` 和 `TSplineTopology.FaceByIndex` 节点收集边集和面集。要创建桥接，面和边以及其中一个曲面用作 `TSplineSurface.BrideFacesToEdges` 节点的输入。这将创建桥接。通过编辑 `spansCounts` 输入，将更多跨度添加到桥接。当曲线用作 `followCurves` 的输入时，桥接遵循所提供曲线的方向。`keepSubdCreases`、`frameRotations`、`firstAlignVertices` 和 `secondAlignVertices` 输入说明如何微调桥接的形状。

## 示例文件

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
