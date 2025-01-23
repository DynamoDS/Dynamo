## 详细
“Mesh.TrainglesAsNineNumbers”确定构成所提供网格中每个三角形的顶点的 X、Y 和 Z 坐标，从而为每个三角形生成九个数字。此节点可用于查询、重建或转换原始网格。

在下面的示例中，“File Path”和“'Mesh.ImportFile”用于导入网格。然后，“Mesh.TrianglesAsNineNumbers”用于获取每个三角形顶点的坐标。然后，使用“List.Chop”并将“lengths”输入设置为 3，将此列表细分为三种形式。然后，“List.GetItemAtIndex”用于获取每个 X、Y 和 Z 坐标，并使用“Point.ByCoordinates”重建顶点。点列表进一步分为三个部分(每个三角形 3 个点)，并用作“Polygon.ByPoints”的输入。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
