<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## 详细
`Curve.ExtrudeAsSolid (curve, distance)` 使用输入数字拉伸输入的闭合平面曲线以确定拉伸的方向。拉伸的方向由曲线所在平面的法线向量确定。此节点对拉伸的末端进行封口以创建实体。

在下面的示例中，我们先使用 `NurbsCurve.ByPoints` 节点(其中一组随机生成的点作为输入)创建 NurbsCurve。然后，`Curve.ExtrudeAsSolid` 节点用于将曲线拉伸为实体。一个数字滑块用作 `Curve.ExtrudeAsSolid` 节点中的 `distance` 输入。
___
## 示例文件

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
