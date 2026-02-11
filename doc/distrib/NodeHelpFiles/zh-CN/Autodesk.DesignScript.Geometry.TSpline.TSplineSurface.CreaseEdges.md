## In-Depth
`TSplineSurface.CreaseEdges` 将尖锐的锐化添加到 T-Spline 曲面上的指定边。
在下面的示例中，从 T-Spline 圆环体生成 T-Spline 曲面。使用 `TSplineTopology.EdgeByIndex` 节点选择边，然后借助 `TSplineSurface.CreaseEdges` 节点将锐化应用于该边。边的两条边上的顶点也会进行锐化。借助 `TSplineEdge.UVNFrame` 和 `TSplineUVNFrame.Poision` 节点，预览选定边的位置。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
