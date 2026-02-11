## 详细
“Curve by IsoCurve on Surface”将创建一条在曲面上为等参曲线的曲线，方法是指定 U 或 V 方向，并指定在创建曲线的相反方向上的参数。“direction”输入确定要创建的等参曲线的方向。值为 1 对应于 U 方向，而值为 0 对应于 V 方向。在下例中，我们先创建点网格，然后在 Z 方向上按随机量平移它们。这些点用于通过使用“NurbsSurface.ByPoints”节点来创建曲面。此曲面用作“ByIsoCurveOnSurface”节点的“baseSurface”。一个设置为 0 到 1 范围且步长为 1 的“数字”滑块用于控制我们是提取 U 方向还是 V 方向的等参曲线。第二个“数字”滑块用于确定提取等参曲线的参数。
___
## 示例文件

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

