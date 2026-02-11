## 详细
在下面的示例中，通过拉伸 NURBS 曲线创建 T-Spline 曲面。使用 `TSplineTopology.EdgeByIndex` 节点选择它的六条边 - 图形的每侧各有三条边。将两组边以及曲面传递给 `TSplineSurface.MergeEdges` 节点。边组的顺序影响形状 - 第一组边进行位移以与第二组边相交，而第二组边仍保留在相同的位置。`insertCreases` 输入添加了沿合并边折缝的选项。合并操作的结果平移到侧面以便更好地预览。
___
## 示例文件

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
