## In-Depth
`TSplineVertex.Info` 返回 T-Spline 顶点的以下特性:
- `uvnFrame`: 外壳线上的点、T-Spline 顶点的 U 向量、V 向量和法线向量
- `index`: T-Spline 曲面上所选顶点的索引
- `isStarPoint`: 所选顶点是否是星形点
- `isTpoint`: 所选顶点是否是 T 点
- `isManifold`: 所选顶点是否是流形顶点
- `valence`: 所选 T-Spline 顶点上的边数
- `functionalValence`: 顶点的功能边价。有关详细信息，请参见 `TSplineVertex.FunctionalValence` 节点的文档。

在下面的示例中，`TSplineSurface.ByBoxCorners` 和 `TSplineTopology.VertexByIndex` 分别用于创建 T-Spline 曲面并选择其顶点。`TSplineVertex.Info` 用于收集有关所选顶点的上述信息。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
