## In-Depth
在下面的示例中，T-Spline 曲面创建为给定轮廓 `curve` 的拉伸。该曲线可以是开放或闭合的。拉伸在提供的 `direction` 方向上执行，并且可以在两个方向上执行，由 `frontDistance` 和 `backDistance` 输入控制。可以使用给定的 `frontSpans` 和 `backSpans` 分别为拉伸的两个方向设置跨度。要沿曲线建立曲面的定义，`profileSpans` 控制面数，`uniform` 以均匀方式分布面或考虑曲率。最后，`inSmoothMode` 控制曲面是以平滑模式还是长方体模式显示。

## 示例文件
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
