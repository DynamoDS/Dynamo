## In-Depth
`TSplineVertex.IsStarPoint` 返回顶点是否是星形点。

当 3 条、5 条或更多条边聚集在一起时，就会出现星形点。它们天生存在于长方体或四分球基本体中，通常是在拉伸 T-Spline 面、删除面或执行“合并”时创建的。与常规顶点和 T 点顶点不同，星形点不受控制点的矩形行控制。星形点使其周围区域更难以控制并可能产生失真，因此只能在必要时使用。星形点放置的不良位置包括模型中较尖锐部分(如锐化边)、曲率变化显著的部分或开放曲面的边上。

星形点还确定 T-Spline 如何转换为边界表示(BREP)。将 T-Spline 转换为 BREP 时，它将在每个星形点处分割为单独的曲面。

在下面的示例中，`TSplineVertex.IsStarPoint` 用于查询使用 `TSplineTopology.VertexByIndex` 选择的顶点是否是星形点。


## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
