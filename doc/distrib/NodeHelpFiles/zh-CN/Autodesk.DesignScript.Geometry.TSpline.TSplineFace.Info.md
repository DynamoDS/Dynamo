## In-Depth
`TSplineFace.Info` 返回 T-Spline 面的以下特性:
- `uvnFrame`: 外壳线上的点、T-Spline 面的 U 向量、V 向量和法线向量
- `index`: 面的索引
- `valence`: 形成面的顶点或边的数量
- `sides`: 每个 T-Spline 面的边数

在下面的示例中，`TSplineSurface.ByBoxCorners` 和 `TSplineTopology.RegularFaces` 用于分别创建 T-Spline 并选择其面。`List.GetItemAtIndex` 用于拾取 T-Spline 的特定面，`TSplineFace.Info` 用于查找其特性。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
