<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## 详细
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` 先在由输入的一列参数确定的点处分割曲线。然后，它返回奇数段或偶数段，这是由 `discardEvenSegments` 输入的布尔值确定的。

在下面的示例中，我先使用 `NurbsCurve.ByControlPoints` 节点(其中一组随机生成的点作为输入)创建 NurbsCurve。一个 `code block` 用于创建一系列介于 0 和 1 之间的数字(步长为 0.1)。将这些数字用作 `Curve.TrimSegmentsByParameter` 节点的输入参数会生成一列曲线，这些曲线实际上是原始曲线的虚线版本。
___
## 示例文件

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
