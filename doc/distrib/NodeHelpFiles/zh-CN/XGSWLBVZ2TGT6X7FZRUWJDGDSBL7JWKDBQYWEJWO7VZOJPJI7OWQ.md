## 详细
`TSplineSurface.FlattenVertices(vertices, parallelPlane)` 通过将一组指定顶点的控制点与作为输入提供的 `parallelPlane` 对齐来更改这些顶点的位置。

在下面的示例中，使用 `TsplineTopology.VertexByIndex` 和 `TSplineSurface.MoveVertices` 节点对 T-Spline 平面曲面的顶点进行位移。然后，将曲面平移到一侧以更好地预览，并将其用作 `TSplineSurface.FlattenVertices(vertices, parallelPlane)` 节点的输入。结果是一个新曲面，其中选定顶点平放在提供的平面上。
___
## 示例文件

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
