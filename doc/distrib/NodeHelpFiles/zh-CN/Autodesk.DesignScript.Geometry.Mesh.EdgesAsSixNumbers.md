## 详细
“Mesh.EdgesAsSixNumbers”确定构成所提供网格中每个唯一边的顶点的 X、Y 和 Z 坐标，因此每条边有六个数字。此节点可用于查询或重建网格或其边。

在下面的示例中，“Mesh.Cuboid”用于创建一个立方体网格，然后将其用作“Mesh.EdgesAsSixNumbers”节点的输入，以检索表示为六个数字的边列表。使用“List.Chop”将列表细分为 6 个项目的列表，然后使用“List.GetItemAtIndex”和“Point.ByCoordinates”重建每条边的起点和终点列表。最后，“List.ByStartPointEndPoint”用于重建网格的边。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
