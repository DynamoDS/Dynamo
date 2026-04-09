## 详细
当需要将闭合 NURBS 曲线导出到另一个系统(例如 Alias)或该系统需要周期性曲线时，请使用“NurbsCurve.PeriodicKnots”。许多 CAD 工具都要求使用这种形式以确保双向转换的准确性。

“PeriodicKnots”返回 *周期性*(非钳制)形式的结向量。“Knots”以*钳制* 形式返回它。两个数组具有相同的长度；它们是描述同一条曲线的两种不同方式。在“钳制”形式中，结在起点和终点重复，因此曲线固定到参数范围。在周期性形式中，结间距在起点和终点重复，从而形成一个平滑的闭环。

在下面的示例中，使用“NurbsCurve.ByControlPointsWeightsKnots”构建周期性 NURBS 曲线。观察节点会比较“Knots”和“PeriodicKnots”，因此可以看到相同的长度，但值不同。“Knots”是钳制形式(末端有重复节点)，“PeriodicKnots”是具有定义曲线周期性的重复差分图案的未钳制形式。
___
## 示例文件

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
