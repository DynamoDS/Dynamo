## 详细
在下面的示例中，使用 `TSplineSurface.FillHole` 节点填充 T-Spline 圆柱曲面中的间隙，该节点需要以下输入:
- `edges`: 拾取自 T-Spline 曲面要填充的边界边数
- `fillMethod`: 0-3 之间的数值，该数值指示填充方法:
    * 0 使用镶嵌细分填充孔
    * 1 使用单个多边形面填充孔
    * 2 在孔的中心创建一个点，三角形面从该点向边缘辐射
    * 3 类似于方法 2，区别是中心顶点结合到一个顶点，而不是仅仅堆叠在顶部。
- `keepSubdCreases`: 指示是否保留细分锐化的布尔值。
___
## 示例文件

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
