## 详细
“Mesh.CloseCracks”通过从网格对象移除内部边界来闭合网格中的裂缝。内部边界可能会作为网格建模操作的结果自然产生。如果移除了退化边，则可在此操作中删除三角形。在下面的示例中，“Mesh.CloseCracks”用于导入的网格。“Mesh.VertexNormals”用于可视化重叠的顶点。原始网格通过 Mesh.CloseCracks 后，边数会减少，这也可以通过使用“Mesh.EdgeCount”节点比较边数来明显看出。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
