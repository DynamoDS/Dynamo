## In-Depth
`TSplineEdge.Info` 返回 T-Spline 曲面边的以下特性:
- `uvnFrame`: 外壳线上的点、T-Spline 边的 U 向量、V 向量和法线向量
- `index`: 边的索引
- `isBorder`: 所选边是否是 T-Spline 曲面的边界
- `isManifold`: 所选边是否是流形边

在下面的示例中，`TSplineTopology.DecomposedEdges` 用于获取 T-Spline 圆柱体基本体曲面的所有边的列表，`TSplineEdge.Info` 用于研究其特性。


## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
