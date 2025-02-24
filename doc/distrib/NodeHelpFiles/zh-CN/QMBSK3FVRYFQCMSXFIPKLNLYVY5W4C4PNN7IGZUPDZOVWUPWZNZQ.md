<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints` 从点列表生成 T-Spline 基本体平面曲面。要创建 T-Spline 平面，该节点使用以下输入:
- `points`: 一组用于定义平面方向和原点的点。如果输入点不在单个平面上，则基于最佳拟合确定平面的方向。创建曲面至少需要三个点。
- `minCorner` 和 `maxCorner`: 平面的角点，表示为具有 X 和 Y 值(将忽略 Z 坐标)的点。如果将输出 T-Spline 曲面平移到 XY 平面，则这些角点表示该曲面的范围。`minCorner` 和 `maxCorner` 点无需与三维中的角顶点相符。
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

在下面的示例中，使用随机生成的点列表创建 T-Spline 平面曲面。曲面的大小由用作`minCorner` 和 `maxCorner`输入的两个点控制。

## 示例文件

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
