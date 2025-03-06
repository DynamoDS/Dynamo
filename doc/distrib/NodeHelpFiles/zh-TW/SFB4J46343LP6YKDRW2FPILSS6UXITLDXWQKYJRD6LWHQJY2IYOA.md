<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint --->
<!--- SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA --->
## In-Depth
`TSplineSurface.ByPlaneLineAndPoint` 使用一條線和一個點產生 T 雲形線基本型平面曲面。產生的 T 雲形線曲面是一個平面。若要建立 T 雲形線平面，節點使用以下輸入:
- `line` 和 `point`: 定義平面的方位和位置所需的輸入。
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

以下範例使用一條線和一個點作為輸入，建立 T 雲形線平面曲面。曲面的大小由 `minCorner` 和 `maxCorner` 輸入的兩點控制。

## 範例檔案

![Example](./SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA_img.jpg)
