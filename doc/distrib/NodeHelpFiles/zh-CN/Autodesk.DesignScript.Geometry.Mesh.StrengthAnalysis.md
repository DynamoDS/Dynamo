## 详细
 “Mesh.StrengthAnalysis”节点返回每个顶点的代表性颜色列表。该结果可以与“Mesh.ByMeshColor”节点一起使用。网格中较强的区域显示为绿色，而较弱的区域用黄色到红色的热图表示。如果网格太粗糙或太不规则(即，有许多长而薄的三角形)，则分析可能会导致误报。您可以尝试使用“Mesh.Remesh”生成常规网格，然后再对其调用“Mesh.StrengthAnalysis”以生成更好的结果。

在下面的示例中，“Mesh.StrengthAnalysis”用于将删格形状的网格的结构强度用颜色编码。结果是与网格顶点长度匹配的颜色列表。此列表可与“Mesh.ByMeshColor”节点一起使用，以为网格着色。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
