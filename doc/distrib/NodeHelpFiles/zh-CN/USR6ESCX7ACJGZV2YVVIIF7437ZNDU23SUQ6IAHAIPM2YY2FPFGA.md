## 详细
`TSplineSurface.Thicken(vector, softEdges)` 加厚由指定向量引导的 T-Spline 曲面。加厚操作在 `vector` 方向上复制曲面，然后通过连接其边来连接两个曲面。`softEdges` 布尔输入控制对生成的边进行平滑(true)还是锐化(false)。

在下面的示例中，使用 `TSplineSurface.Thicken(vector, softEdges)` 节点加厚 T-Spline 拉伸曲面。生成的曲面平移到一侧以更好地可视化。


___
## 示例文件

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
