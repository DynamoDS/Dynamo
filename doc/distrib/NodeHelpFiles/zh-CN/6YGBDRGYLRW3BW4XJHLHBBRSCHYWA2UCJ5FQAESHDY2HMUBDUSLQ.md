<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
`TSplineSurface.AddReflections` 通过将一个或多个反射应用于输入 `tSplineSurface` 来创建新的 T-Spline 曲面。布尔输入 `weldSymmetricPortions` 确定是平滑还是保留反射生成的锐化边。

以下示例说明如何使用 `TSplineSurface.AddReflections` 节点将多个反射添加到 T-Spline 曲面。创建两个反射 - 轴向和径向。基础几何图形是路径为圆弧的扫略形状的 T-Spline 曲面。这两个反射加入列表，并与要反射的基础几何图形一起用作 `TSplineSurface.AddReflections` 节点的输入。TSplineSurface 将进行接合，从而生成光滑的 TSplineSurface，而不会出现锐化边。通过使用 `TSplineSurface.MoveVertex` 节点移动一个顶点，从而进一步改变曲面。由于反射应用于 T-Spline 曲面，因此移动顶点将重复 16 次。

## 示例文件

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
