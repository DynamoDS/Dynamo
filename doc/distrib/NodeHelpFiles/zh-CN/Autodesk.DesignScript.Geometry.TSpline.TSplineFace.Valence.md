## In-Depth
在下面的示例中，`TSplineFace.Valence` 用于查询 T-Spline 圆锥体基本体的所有面的边价。请注意，所有面(包括圆锥体的上行)都返回边价值 4，即使上行看起来可能由三角形组成也是如此。这特定于几个 T-Spline 基本体形状(如圆锥体和球体)，其中，为了实现该形状，将四个顶点中的两个堆叠在一个点上。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Valence_img.jpg)
