## 详细
“Mesh.MakeHollow”操作可用于挖空网格对象，以准备进行三维打印。空心网格可以显著减少所需的打印材料量、打印时间和成本。“wallThickness”输入定义网格对象的壁厚。(可选)“Mesh.MakeHollow”可以生成逃生孔，以在打印过程中去除多余的材料。孔的大小和数量由输入“holeCount”和“holeRadius”控制。最后，“meshResolution”和“solidResolution”的输入会影响网格结果的分辨率。较高的“meshResolution”可以提高网格内部偏移原始网格的精度，但会产生更多的三角形。较高的“solidResolution”可以提高原始网格的更精细细节在空心网格内部部分的保留程度。
在下面的示例中，“Mesh.MakeHollow”用于圆锥体形状的网格。在其底部增加了五个逃生孔。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
