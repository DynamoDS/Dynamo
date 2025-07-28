<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## 详细
`TSplineSurface.CompressIndexes` 节点删除 T-Spline 曲面的边、顶点或面的索引编号中的间隙，这些间隙因“删除面”等各种操作产生。将保留索引的顺序。

在下面的示例中，将从四分球基本体曲面中删除许多面，这将影响形状的边索引。`TSplineSurface.CompressIndexes` 用于修复形状的边索引，因此可以选择索引为 1 的边。

## 示例文件

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
