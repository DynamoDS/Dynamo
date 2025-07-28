## 详细
`TSplineTopology.BorderEdges` 返回 T-Spline 曲面中包含的边界边的列表。

在下面的示例中，通过 `TSplineSurface.ByCylinderPointsRadius` 创建两个 T-Spline 曲面；一个是开放曲面，而另一个是使用 `TSplineSurface.Thicken` 加厚的曲面，这会将其变为闭合曲面。当使用 `TSplineTopology.BorderEdges` 节点检查这两个曲面时，第一个曲面返回一个边界边列表，而第二个曲面返回一个空列表。这是因为由于曲面是封闭的，因此没有边界边。
___
## 示例文件

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)
