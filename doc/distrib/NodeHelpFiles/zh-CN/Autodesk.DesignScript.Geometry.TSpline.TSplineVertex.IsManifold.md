## In-Depth
在下面的示例中，通过连接两个共享内部边的曲面生成一个非流形曲面。结果是一个没有清晰正面和背面的曲面。在修复非流形曲面之前，该曲面只能在长方体模式下显示。`TSplineTopology.DecomposedVertices` 用于查询曲面的所有顶点，`TSplineVertex.IsManifold` 节点用于亮显哪些顶点被视为流形顶点。通过使用 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 节点，提取非流形顶点并可视化其位置。


## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)
