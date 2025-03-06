<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
在下面的示例中，`TSplineSurface.UncreaseVertices` 节点用于平面基本体的角顶点。默认情况下，在创建曲面时对这些顶点进行锐化。借助 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Poision` 节点以及激活的 `Show Labels` 选项，识别这些顶点。然后，使用 `TSplineTopology.VertexByIndex` 节点选择角顶点并取消锐化。如果形状处于平滑模式预览，则可以预览此操作的效果。

## 示例文件

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
