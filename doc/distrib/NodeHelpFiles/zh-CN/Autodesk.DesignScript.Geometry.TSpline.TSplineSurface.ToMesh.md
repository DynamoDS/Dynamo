## 详细
在下面的示例中，使用 `TSplineSurface.ToMesh` 节点将一个简单的 T-Spline 长方体曲面转换为网格。`minSegments` 输入定义每个方向上面的最小分段数，对于控制网格定义非常重要。`tolerance` 输入通过添加更多顶点位置以在给定公差内匹配原始曲面来校正不准确性。结果是一个网格，其定义使用 `Mesh.VertexPositions` 节点进行预览。
输出网格可以包含三角形和四边形，在使用 MeshToolkit 节点时请务必记住这一点。
___
## 示例文件

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
