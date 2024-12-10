## 详细
“Mesh.ByGeometry”将 Dynamo 几何图形对象(曲面或实体)作为输入，并将其转换为网格。点和曲线没有网格表示，因此它们不是有效输入。在转换中生成的网格的分辨率由两个输入(“tolerance”和“maxGridLines”)控制。“tolerance”用于设置网格与原始几何图形的可接受偏差，并受网格尺寸的影响。如果“tolerance”值设置为 -1，则 Dynamo 会选择合理的公差。“maxGridLines”输入设置 U 或 V 方向上栅格线的最大数量。更多的栅格线有助于提高细分的平滑度。

## 示例文件

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
