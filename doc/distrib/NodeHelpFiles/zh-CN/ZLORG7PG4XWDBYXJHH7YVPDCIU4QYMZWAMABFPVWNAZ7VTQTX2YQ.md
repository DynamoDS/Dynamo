<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseVertices --->
<!--- ZLORG7PG4XWDBYXJHH7YVPDCIU4QYMZWAMABFPVWNAZ7VTQTX2YQ --->
## In-Depth
在下面的示例中，使用 `TSplineSurface.BuildFromLines` 节点并将 `creaseOuterVertices` 输入设置为 False，来创建一个边已取消锐化的形状。然后，使用 `TSplineSurface.CreaseVertices` 节点对索引为 1 的顶点进行锐化，然后将该形状平移到右侧以更好地查看。

## 示例文件

![Example](./ZLORG7PG4XWDBYXJHH7YVPDCIU4QYMZWAMABFPVWNAZ7VTQTX2YQ_img.jpg)
