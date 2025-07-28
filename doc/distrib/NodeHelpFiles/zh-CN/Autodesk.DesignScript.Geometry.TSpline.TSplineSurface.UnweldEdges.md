## In-Depth

在下面的示例中，对 T-Spline 曲面的边行执行“拆分”操作。结果，选定边的顶点不连接。与在保持连接的同时在边周围创建尖锐过渡的“取消锐化”不同，“拆分”会创建不连续性。这可以通过比较执行操作之前和之后的顶点数来证明。对拆分边或顶点的任何后续操作也将证明曲面沿拆分边断开连接。

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
