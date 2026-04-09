## 详细
当需要将闭合 NURBS 曲线导出到另一个系统(例如 Alias)或该系统需要周期性曲线时，请使用“NurbsCurve.PeriodicControlPoints”。许多 CAD 工具都要求使用这种形式以确保双向转换的准确性。

“PeriodicControlPoints”以*周期性*形式返回控制点。“ControlPoints”以*钳制*形式返回它们。两个数组具有相同数量的点；它们是描述同一条曲线的两种不同方式。在周期性形式中，最后几个控制点与前几个控制点匹配(与曲线阶数一样多)，因此曲线平滑闭合。钳制形式使用不同的布局，因此两个数组中的点位置不同。

在下面的示例中，使用“NurbsCurve.ByControlPointsWeightsKnots”构建周期性 NURBS 曲线。观察节点会比较“ControlPoints”和“PeriodicControlPoints”，因此可以看到相同长度但点位置不同。控制点以红色显示，因此它们与背景预览中黑色的周期性控制点的显示截然不同。
___
## 示例文件

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
