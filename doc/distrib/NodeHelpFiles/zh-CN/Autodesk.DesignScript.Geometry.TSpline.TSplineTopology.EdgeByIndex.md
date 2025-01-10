## 详细
在下面的示例中，使用 `TSplineSurface.ByBoxLengths` 节点以及指定的原点、宽度、长度、高度、跨度和对称创建一个 T-Spline 长方体。
然后，`EdgeByIndex` 用于从所生成曲面中的边列表中选择一条边。接着，使用 `TSplineSurface.SlideEdges` 使选定边沿相邻边滑动，随后是其对称对应边。
___
## 示例文件

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
