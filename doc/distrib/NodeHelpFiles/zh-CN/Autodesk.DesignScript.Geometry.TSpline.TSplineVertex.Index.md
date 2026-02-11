## In-Depth
`TSplineVertex.Index` 返回 T-Spline 曲面上所选顶点的索引编号。请注意，在 T-Spline 曲面拓扑中，面、边和顶点的索引不一定与列表中项目的序号相符。使用 `TSplineSurface.CompressIndices` 节点解决此问题。

在下面的示例中，`TSplineTopology.StarPointVertices` 用于长方体形状的 T-Spline 基本体。然后，`TSplineVertex.Index` 用于查询星形点顶点的索引，`TSplineTopology.VertexByIndex` 返回选定顶点以供进一步编辑。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
