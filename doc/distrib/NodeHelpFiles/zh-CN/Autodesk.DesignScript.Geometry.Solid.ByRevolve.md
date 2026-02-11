## 详细
`Solid.ByRevolve` 通过围绕轴旋转给定轮廓曲线来创建曲面。该轴由 `axisOrigin` 点和 `axisDirection` 向量进行定义。起始角度确定曲面的起始位置(以度为测量单位)，`sweepAngle` 确定围绕轴继续曲面的距离。

在下面的示例中，我们使用由余弦函数生成的曲线作为轮廓曲线，并使用两个数字滑块来控制 `startAngle` 和 `sweepAngle`。在本示例中，`axisOrigin` 和 `axisDirection` 保留为世界原点和世界 z 轴的默认值。

___
## 示例文件

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

