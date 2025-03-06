<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## 详细
在下面的示例中，使用 `TSplineTopology.DecomposedVertices` 节点检查具有拉伸、细分和拉动顶点和面的平面 T-Spline 曲面，这将返回在 T-Spline 曲面中包含的以下顶点类型的列表:

- `all`: 所有顶点的列表
- `regular`: 常规顶点的列表
- `tPoints`: T 点顶点的列表
- `starPoints`: 星形点顶点的列表
- `nonManifold`: 非流形顶点的列表
- `border`: 边界顶点的列表
- `inner`: 内部顶点的列表

`TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 节点用于亮显曲面的不同类型的顶点。

___
## 示例文件

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
