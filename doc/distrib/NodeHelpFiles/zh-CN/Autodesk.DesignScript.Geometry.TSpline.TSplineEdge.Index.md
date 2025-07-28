## In-Depth
请注意，在 T-Spline 曲面拓扑中，`Face`、`Edge` 和 `Vertex` 的索引不一定与列表中项目的序号相符。使用 `TSplineSurface.CompressIndices` 节点解决此问题。

在下面的示例中，`TSplineTopology.DecomposedEdges` 用于检索 T-Spline 曲面的边界边，然后 `TSplineEdge.Index` 节点用于获取所提供边的索引。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
