## 详细
`Cuboid.Height` 返回输入立方体的高度。请注意，如果立方体已使用比例因子转换为其他坐标系，则这将返回立方体的原始尺寸，而不是世界空间尺寸。换句话说，如果创建一个宽度(X 轴)为 10 的立方体并将其转换为一个 CoordinateSystem (在 X 方向上缩放 2 倍)，则宽度仍为 10。

在下面的示例中，我们通过角点生成一个立方体，然后使用 `Cuboid.Height` 节点来查找其高度。

___
## 示例文件

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

