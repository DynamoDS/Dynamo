## 详细
`Curve.Extrude (curve, direction)` 使用输入向量拉伸输入曲线以确定拉伸的方向。向量的长度用于拉伸距离。

在下面的示例中，我们先使用 `NurbsCurve.ByControlPoints` 节点(其中一组随机生成的点作为输入)创建 NurbsCurve。一个代码块用于指定 `Vector.ByCoordinates` 节点的 X、Y 和 Z 分量。然后，此向量用作 `Curve.Extrude` 节点中的 `direction` 输入。
___
## 示例文件

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
