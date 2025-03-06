<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## 详细
`Curve.ExtrudeAsSolid (curve, direction)` 使用输入向量拉伸输入的闭合平面曲线以确定拉伸的方向。向量的长度用于拉伸距离。此节点对拉伸的末端进行封口以创建实体。

在下面的示例中，我们先使用 `NurbsCurve.ByPoints` 节点(其中一组随机生成的点作为输入)创建 NurbsCurve。一个代码块用于指定 `Vector.ByCoordinates` 节点的 X、Y 和 Z 分量。然后，此向量用作 `Curve.ExtrudeAsSolid` 节点中的方向输入。
___
## 示例文件

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
