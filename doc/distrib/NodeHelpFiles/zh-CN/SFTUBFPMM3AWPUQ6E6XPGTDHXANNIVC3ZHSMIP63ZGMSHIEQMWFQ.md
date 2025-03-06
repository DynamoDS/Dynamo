<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
`TSplineSurface.ByPlaneThreePoints` 使用三个点作为输入生成一个 T-Spline 基本体平面曲面。要创建 T-Spline 平面，该节点使用以下输入:
- `p1`、`p2` 和 `p3`: 定义平面位置的三个点。第一个点被视为平面的原点。
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

在下面的示例中，T-Spline 平面曲面由三个随机生成的点创建。第一个点是平面的原点。曲面的大小由用作`minCorner` 和 `maxCorner` 输入的两个点控制。

## 示例文件

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
