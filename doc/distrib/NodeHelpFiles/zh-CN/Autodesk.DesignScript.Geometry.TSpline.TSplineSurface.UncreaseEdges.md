## In-Depth
与 `TSplineSurface.CreaseEdges` 节点相反，此节点删除 T-Spline 曲面上指定边的锐化。
在下面的示例中，从 T-Spline 圆环体生成 T-Spline 曲面。使用 `TSplineTopology.EdgeByIndex` 和 `TSplineTopology.EdgesCount` 节点选择所有边，并借助 `TSplineSurface.CreaseEdges` 节点将锐化应用于所有边。然后，选择索引为 0 到 7 的边的子集，并应用反向操作 - 这次使用 `TSplineSurface.UncreaseEdges` 节点。借助 `TSplineEdge.UVNFrame` 和 `TSplineUVNFrame.Poision` 节点预览选定边的位置。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
