<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis --->
<!--- JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA --->
## In-Depth
`TSplineSurface.ByPlaneOriginXAxisYAxis` 使用原点和两个向量(表示平面的 X 轴和 Y 轴)生成一个 T-Spline 基本体平面曲面。要创建 T-Spline 平面，该节点使用以下输入:
- `origin`: a point defining the origin of the plane.
- `xAxis` 和 `yAxis`: 定义所创建平面的 X 轴和 Y 轴方向的向量。
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

在下面的示例中，使用提供的原点和两个向量(用作 X 和 Y 方向)创建一个 T-Spline 平面曲面。曲面的大小由用作 `minCorner` 和 `maxCorner` 输入的两个点控制。

## 示例文件

![Example](./JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA_img.jpg)
