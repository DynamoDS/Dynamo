## 详细
此节点对所提供网格中的边数进行计数。如果网格由三角形组成(这是“MeshToolkit”中所有网格的情况)，则“Mesh.EdgeCount”节点仅返回唯一边。因此，可以预期边的数量不会是网格中三角形数量的三倍。此假定可用于验证网格不包含任何未焊接的面(可能出现在导入的网格中)。

在下面的示例中，“Mesh.Cone”和“Number.Slider”用于创建一个圆锥体，然后将其用作计数边的输入。“Mesh.Edges”和“Mesh.Triangles”都可用于在预览中预览网格的结构和网格，“Mesh.Edges”在复杂和重型网格中显示出更好的性能。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
