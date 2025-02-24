## In-Depth
`TSplineVertex.IsTPoint` 返回顶点是否是 T 点。T 点是控制点部分行末尾处的顶点。

在下面的示例中，`TSplineSurface.SubdivideFaces` 用于 T-Spline 长方体基本体，以举例说明将 T 点添加到曲面的多种方法之一。`TSplineVertex.IsTPoint` 节点用于确认索引处的顶点是否是 T 点。为了更好地可视化 T 的位置，使用 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 节点。



## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
