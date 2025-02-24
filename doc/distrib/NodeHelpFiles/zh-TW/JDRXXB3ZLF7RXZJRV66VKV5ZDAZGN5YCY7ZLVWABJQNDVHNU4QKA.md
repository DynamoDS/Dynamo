<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis --->
<!--- JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA --->
## In-Depth
`TSplineSurface.ByPlaneOriginXAxisYAxis` 使用原點和表示平面 X 軸和 Y 軸的兩個向量，產生 T 雲形線基本型平面曲面。若要建立 T 雲形線平面，節點使用以下輸入:
- `origin`: a point defining the origin of the plane.
- `xAxis` 和 `yAxis`: 定義所建立平面 X 和 Y 軸方向的向量。
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

以下範例使用提供的原點和當作 X 方向和 Y 方向的兩個向量建立 T 雲形線平面曲面。曲面的大小由 `minCorner` 和 `maxCorner` 輸入的兩點控制。

## 範例檔案

![Example](./JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA_img.jpg)
