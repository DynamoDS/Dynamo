## 详细
在下面的示例中，创建一个简单的 T-Spline 长方体曲面，并使用 `TSplineTopology.EdgeByIndex` 节点选择它的一条边。为了更好地了解所选顶点的位置，借助 `TSplineEdge.UVNFrame` 和 `TSplineUVNFrame.Position` 节点可视化它。所选边以及其所属的曲面作为 `TSplineSurface.SlideEdges` 节点的输入进行传递。`amount` 输入确定边向其相邻边滑动的程度，以百分比表示。`roundness` 输入控制倒角的平坦度或圆度。在长方体模式下更好地了解圆度的效果。然后，滑动操作的结果平移到侧面以进行预览。

___
## 示例文件

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
