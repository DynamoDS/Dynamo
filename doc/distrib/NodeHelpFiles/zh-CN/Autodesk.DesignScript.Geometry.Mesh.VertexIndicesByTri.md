## 详细
“Mesh.VertexIndicesByTri”返回与每个网格三角形对应的顶点索引的展平列表。索引按三分排列，可以使用“List.Chop”节点和“lengths”输入 3 轻松重建索引分组。

在下面的示例中，包含 20 个三角形的“MeshToolkit.Mesh”被转换为“Geometry.Mesh”。“Mesh.VertexIndicesByTri”用于获取索引列表，然后使用“List.Chop”将其划分为包含三个的列表。使用“List.Transpose”翻转列表结构，以获得三个顶层列表，其中包含对应于每个网格三角形中的点 A、B 和 C 的 20 个索引。“IndexGroup.ByIndices”节点用于创建三个索引中每个的索引组。然后，将“IndexGroups”的结构化列表和顶点列表用作“Mesh.ByPointsFaceIndices”的输入，以获取转换后的网格。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
