## 详细
“Mesh.Cone”创建一个网格圆锥体，其底部以输入原点为中心，具有底部和顶部半径、高度和许多“divisions”的输入值。“divisions”的数量对应于在圆锥体顶部和底部创建的顶点数量。如果“divisions”数为 0，则 Dynamo 使用默认值。沿 Z 轴的分割数始终等于 5。“cap”输入使用“Boolean”来控制圆锥体是否在顶部闭合。
在下面的示例中，“Mesh.Cone”节点用于创建具有 6 个分割的圆锥体形状的网格，因此圆锥体的底部和顶部是六边形。“Mesh.Triangles”节点用于可视化网格三角形的分布。


## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
