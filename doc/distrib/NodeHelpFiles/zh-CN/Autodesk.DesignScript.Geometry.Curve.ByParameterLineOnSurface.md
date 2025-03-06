## 详细
“Curve by Parameter Line On Surface”将沿两个输入 UV 坐标之间的曲面创建一条线。在下例中，我们先创建点网格，然后在 Z 方向上按随机量平移它们。这些点用于通过使用“NurbsSurface.ByPoints”节点来创建曲面。此曲面用作“ByParameterLineOnSurface”节点的“baseSurface”。一组“数字”滑块用于调整两个“UV.ByCoordinates”节点的 U 和 V 输入，然后用于确定曲面上线的起点和终点。
___
## 示例文件

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

