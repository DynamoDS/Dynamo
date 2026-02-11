<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormal` 使用原点和法线向量生成 T-Spline 基本体平面曲面。要创建 T-Spline 平面，该节点使用以下输入:
- `origin`: 定义平面原点的点。
- `normal`: 指定所创建平面的法线方向的向量。
- `minCorner` 和 `maxCorner`: 平面的角点，表示为具有 X 和 Y 值(将忽略 Z 坐标)的点。如果将输出 T-Spline 曲面平移到 XY 平面，则这些角点表示该曲面的范围。`minCorner` 和 `maxCorner` 点无需与三维中的角顶点相符。例如，当 `minCorner` 设置为 (0,0) 且 `maxCorner` 为 (5,10) 时，平面宽度和长度将分别为 5 和 10。
- `xSpans` 和 `ySpans`: 平面的宽度和长度跨度数/分割数
- `symmetry`: 几何图形是否相对于其 X、Y 和 Z 轴对称
- `inSmoothMode`: 生成的几何图形是以平滑模式还是长方体模式显示

在下面的示例中，使用提供的原点和法线(X 轴的向量)创建 T-Spline 平面曲面。曲面的大小由用作 `minCorner` 和 `maxCorner` 输入的两个点控制。

## 示例文件

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
