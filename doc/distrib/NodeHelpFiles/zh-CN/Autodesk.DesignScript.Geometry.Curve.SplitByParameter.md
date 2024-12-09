## 详细
`Curve.SplitByParameter (curve, parameters)` 将一条曲线和一列参数用作输入。它在指定的参数处分割曲线，并返回一列结果曲线。

在下面的示例中，我们先使用 `NurbsCurve.ByControlPoints` 节点(其中一组随机生成的点作为输入)创建 NurbsCurve。一个代码块用于创建一系列介于 0 和 1 之间的数字，以用作分割曲线的参数列表。

___
## 示例文件

![SplitByParameter](./Autodesk.DesignScript.Geometry.Curve.SplitByParameter_img.jpg)

