## 详细
“Pull Onto Surface”将通过将输入曲线投影到输入曲面上并使用曲面的法线向量作为投影方向，来创建一条新曲线。在下例中，我们先使用“Surface.BySweep”节点创建曲面，该节点使用根据正弦曲线生成的曲线。此曲面用作要在“PullOntoSurface”中拉入的基础曲面。对于曲线，我们使用“Code Block”指定中心点的坐标并使用“数字”滑块控制圆的半径，从而创建一个圆。结果是圆投影到曲面上。
___
## 示例文件

![PullOntoSurface](./Autodesk.DesignScript.Geometry.Curve.PullOntoSurface_img.jpg)

