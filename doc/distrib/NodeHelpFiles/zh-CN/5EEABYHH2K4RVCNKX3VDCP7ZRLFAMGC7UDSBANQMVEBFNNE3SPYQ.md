<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## 详细
`Curve.NormalAtParameter (curve, param)` 返回在曲线的指定参数处与法线方向对齐的向量。曲线的参数化是在 0 到 1 的范围内测量的，其中 0 表示曲线的起点，1 表示曲线的终点。

在下面的示例中，我们先使用 `NurbsCurve.ByControlPoints` 节点(其中一组随机生成的点作为输入)创建 NurbsCurve。一个设置为范围 0 到 1 的数字滑块用于控制 `Curve.NormalAtParameter` 节点的 `parameter` 输入。
___
## 示例文件

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
