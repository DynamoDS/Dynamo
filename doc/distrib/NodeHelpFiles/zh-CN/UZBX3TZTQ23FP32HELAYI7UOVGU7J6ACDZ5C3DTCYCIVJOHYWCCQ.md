<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## 详细
`TSplineSurface.BuildFromLines` 提供了一种创建更复杂 T-Spline 曲面的方法，该曲面既可以用作最终几何图形，也可以用作比默认基本体更接近所需形状的自定义基本体。结果可以是闭合或开放的曲面，并可以有孔和/或锐化边。

节点的输入是表示 TSpline 曲面的“控制框架”的曲线列表。设置直线列表需要进行一些准备，必须遵循特定准则。
- 直线不得重叠
- 多边形的边界必须闭合，并且每条线端点必须至少与另一个端点相交。每条线的交点必须在一个点处相交。
- 对于更详细的区域，需要密度更大的多边形
- 四边形优先于三角形和多边形，因为它们更易于控制。

在下面的示例中，创建两个 T-Spline 曲面以说明此节点的使用。在这两种情况下，`maxFaceValence` 保留为默认值，调整 `snappingTolerance` 以确保公差值范围内的线被视为连接。对于左侧的图形，`creaseOuterVertices` 设置为 False 以使两个角顶点保持尖锐且不圆化。左侧的形状没有外部顶点，此输入保留为默认值。为了平滑预览，为这两个形状激活 `inSmoothMode`。

___
## 示例文件

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
