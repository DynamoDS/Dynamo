## 详细
“Patch”将尝试使用输入曲线作为边界来创建一个曲面。输入曲线必须闭合。在下例中，我们先使用“Point.ByCylindricalCoordinates”节点来以设置的间隔在圆中创建一组点，但使用的是随机高程和半径。然后，我们使用“NurbsCurve.ByPoints”节点来基于这些点创建一条闭合曲线。“Patch”节点用于从边界闭合曲线创建一个曲面。请注意，由于点是使用随机半径和高程创建的，因此并非所有排列都会生成能够修补的曲线。
___
## 示例文件

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

