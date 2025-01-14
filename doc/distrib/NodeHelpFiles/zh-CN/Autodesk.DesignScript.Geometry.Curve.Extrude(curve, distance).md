## 详细
`Curve.Extrude (curve, distance)` 使用输入数字拉伸输入曲线以确定拉伸的距离。法线向量沿曲线的方向用于拉伸方向。

在下面的示例中，我们先使用 `NurbsCurve.ByControlPoints` 节点(其中一组随机生成的点作为输入)创建 NurbsCurve。然后，我们使用 `Curve.Extrude` 节点来拉伸曲线。一个数字滑块用作 `Curve.Extrude` 节点中的 `distance` 输入。
___
## 示例文件

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
